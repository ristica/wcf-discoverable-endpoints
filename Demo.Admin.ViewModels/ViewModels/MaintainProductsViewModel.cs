using Core.Common.UI.Core;
using System.Collections.ObjectModel;
using Demo.Client.Contracts;
using System;
using GalaSoft.MvvmLight.Messaging;
using Demo.Admin.Messages;
using System.ComponentModel.Composition;
using Core.Common.Contracts;
using Demo.Client.Entities;
using Core.Common;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Linq;

namespace Demo.Admin.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MaintainProductsViewModel : ViewModelBase
    {
        #region Fields

        private readonly IServiceFactory _serviceFactory;
        private ObservableCollection<Product> _products;
        private EditProductDialogViewModel _editProductDialog;
        private Product _selectedProduct;
        private bool _isServiceOnline;
        private EndpointAddress _discoveredAddress;
        private ServiceHost _announcementService;

        #endregion

        #region Properties

        public EditProductDialogViewModel EditProductDialog
        {
            get { return this._editProductDialog; }
            set
            {
                if (this._editProductDialog == value) return;
                this._editProductDialog = value;
                OnPropertyChanged(() => this.EditProductDialog);
            }
        }

        public ObservableCollection<Product> Products
        {
            get { return this._products; }
            set
            {
                if (this._products == value) return;
                this._products = value;
                OnPropertyChanged(() => this.Products);
            }
        }

        public Product SelectedProduct
        {
            get { return this._selectedProduct; }
            set
            {
                if (this._selectedProduct == value) return;
                this._selectedProduct = value;
                OnPropertyChanged(() => this.SelectedProduct);
            }
        }

        public bool IsServiceOnline
        {
            get { return this._isServiceOnline; }
            set
            {
                if (this._isServiceOnline == value) return;
                this._isServiceOnline = value;
                OnPropertyChanged(() => this.IsServiceOnline);                
            }
        }

        #endregion

        #region Events

        public event EventHandler<ErrorMessageEventArgs> ErrorOccured;

        #endregion

        #region Commands

        public DelegateCommand<Product> EditProductCommand { get; private set; }
        public DelegateCommand<object> AddProductCommand { get; private set; }
        public DelegateCommand<Product> DeactivateProductCommand { get; private set; }
        public DelegateCommand<Product> ActivateProductCommand { get; private set; }

        #endregion

        #region Overrides

        public override string ViewTitle
        {
            get
            {
                return "Products";
            }
        }

        protected override void OnViewLoaded()
        {
            this._products = new ObservableCollection<Product>();

            // choose one option of discovering the endpoint and 
            // the rest will be bootstrapped

            //// 1) set find criteria by yourself
            //this.LoadProductsWithDiscoveringEndpointWithSettings();

            //// 2) do automatically search
            //this.LoadProductsWithDynamicallyEndpoint();

            //// 3) send notifications to the client that host is (down / up)
            //this.LoadProductsWithDynamicallyEndpointAndAnnouncement();
        }

        #endregion

        #region C-Tor

        [ImportingConstructor]
        public MaintainProductsViewModel(IServiceFactory serviceFactory)
        {
            this._serviceFactory = serviceFactory;

            this.RegisterCommands();
            this.RegisterMessengers();
        }

        #endregion

        #region Methods

        private void RegisterMessengers()
        {
            Messenger.Default.Register<ProductChangedMessage>(this, this.ReloadProducts);
        }

        private void ReloadProducts(ProductChangedMessage message)
        {
            this.Products.Clear();
            var products = this._serviceFactory.CreateClient<IInventoryService>().GetProducts();
            foreach( var p in products)
            {
                this.Products.Add(p);
            }
        }

        private void RegisterCommands()
        {
            this.EditProductCommand = new DelegateCommand<Product>(OnEditProductCommand, CanExecuteEditProductCommand);
            this.AddProductCommand = new DelegateCommand<object>(OnAddProductCommand, CanExecuteAddProductCommand);
            this.DeactivateProductCommand = new DelegateCommand<Product>(OnDeactivateProductCommand, CanExecuteDeactivateProductCommand);
            this.ActivateProductCommand = new DelegateCommand<Product>(OnActivateProductCommand, CanExecuteActivateProductCommand);
        }

        #endregion

        #region Discovering dynamically for service with announcement

        private void LoadProductsWithDynamicallyEndpointAndAnnouncement()
        {
            this.DiscoverServices();
            this.CreateAnnouncementService();

            var proxy = this._serviceFactory.CreateClient<IInventoryService>("dynamicInventoryService");
            if (proxy == null)
            {
                this.IsServiceOnline = false;
                this.CanExecuteAddProductCommand(null);
                return;
            }

            var products = proxy.GetProducts();
            if (products != null && products.Length > 0)
            {
                foreach (var p in products)
                {
                    this._products.Add(p);
                }
            }

            this.IsServiceOnline = true;
            this.CanExecuteAddProductCommand(null);

            // do housekeeping by yourself
            ((IDisposable)proxy).Dispose();
        }

        private void CreateAnnouncementService()
        {
            var announcementService = new AnnouncementService();

            announcementService.OnlineAnnouncementReceived += (sender, args) =>
            {
                if (args.EndpointDiscoveryMetadata.ContractTypeNames.FirstOrDefault(i => i.Name.Equals("IInventoryService")) == null) return;
                this._discoveredAddress = args.EndpointDiscoveryMetadata.Address;
                this.IsServiceOnline = true;

                this.CanExecuteAddProductCommand(null);
            };

            announcementService.OfflineAnnouncementReceived += (sender, args) =>
            {
                if (args.EndpointDiscoveryMetadata.ContractTypeNames.FirstOrDefault(i => i.Name.Equals("IInventoryService")) == null) return;
                this._discoveredAddress = null;
                this.IsServiceOnline = false;

                this.CanExecuteAddProductCommand(null);
            };

            this._announcementService = new ServiceHost(announcementService);
            this._announcementService.Open();
        }

        #endregion

        #region Discovering dynamically for service

        /// <summary>
        /// use it if you know everything about the service but the address
        /// </summary>
        private void LoadProductsWithDynamicallyEndpoint()
        {
            var proxy = this._serviceFactory.CreateClient<IInventoryService>("dynamicInventoryService");
            if (proxy == null)
            {
                this.IsServiceOnline = false;
                this.CanExecuteAddProductCommand(null);
                return;
            }

            var products = proxy.GetProducts();
            if (products != null && products.Length > 0)
            {
                foreach (var p in products)
                {
                    this._products.Add(p);
                }
            }

            this.IsServiceOnline = true;
            this.CanExecuteAddProductCommand(null);

            // do housekeeping by yourself
            ((IDisposable)proxy).Dispose();
        }

        #endregion

        #region Discovering for service with settings

        /// <summary>
        /// use it when you don't know anything about the servce
        /// except the contract
        /// </summary>
        private void LoadProductsWithDiscoveringEndpointWithSettings()
        {
            this.DiscoverServices();

            var proxy = this.CreateInventoryProxy();
            if (proxy == null) return;

            var products = proxy.GetProducts();
            if (products != null && products.Length > 0)
            {
                foreach (var p in products)
                {
                    this._products.Add(p);
                }
            }

            // do housekeeping by yourself
            ((IDisposable)proxy).Dispose();
        }

        private void DiscoverServices()
        {
            var discoveryProxy = new DiscoveryClient(new UdpDiscoveryEndpoint());

            var findCriteria = new FindCriteria(typeof(IInventoryService))
            {
                MaxResults = 1,
                Duration = new TimeSpan(0, 0, 5)
            };

            // scope => just another filter criteria
            // if we have more then one implementation of the contract
            findCriteria.Scopes.Add(new Uri("http://www.xxx.com/pingo/demo/discoverability"));

            var findResponse = discoveryProxy.Find(findCriteria);

            if (findResponse.Endpoints.Count > 0)
            {
                EndpointDiscoveryMetadata discoveredEndpoint = findResponse.Endpoints[0];
                this._discoveredAddress = discoveredEndpoint.Address;

                this.IsServiceOnline = true;
            }
            else
            {
                this._discoveredAddress = null;
                this.IsServiceOnline = false;
            }

            this.CanExecuteAddProductCommand(null);
        }

        private IInventoryService CreateInventoryProxy()
        {
            if (this._discoveredAddress == null)
            {
                return null;
            }

            if (this._discoveredAddress.ToString().ToLower().StartsWith("net.tcp"))
            {
                var binding = new NetTcpBinding();

                var factory = new ChannelFactory<IInventoryService>(binding, this._discoveredAddress);
                var proxy = factory.CreateChannel();
                return proxy;
            }
            else
            {
                var binding = new WSHttpBinding();

                var factory = new ChannelFactory<IInventoryService>(binding, this._discoveredAddress);
                var proxy = factory.CreateChannel();
                return proxy;
            }
        }

        #endregion

        #region CanExecute... Command

        private bool CanExecuteActivateProductCommand(Product obj)
        {
            return this.IsServiceOnline;
        }

        private bool CanExecuteDeactivateProductCommand(Product obj)
        {
            return this.IsServiceOnline;
        }

        private bool CanExecuteAddProductCommand(object obj)
        {
            return this.IsServiceOnline;
        }

        private bool CanExecuteEditProductCommand(Product obj)
        {
            return this.IsServiceOnline;
        }

        #endregion

        #region On...Command

        private void OnEditProductCommand(Product product)
        {
            this.EditProductDialog = Container.GetExportedValue<EditProductDialogViewModel>();
            this.EditProductDialog.Title = "Edit product...";
            this.EditProductDialog.Model = product;
            this.EditProductDialog.IsOpen = true;
        }

        private void OnAddProductCommand(object obj)
        {
            this.EditProductDialog = Container.GetExportedValue<EditProductDialogViewModel>();
            this.EditProductDialog.Title = "Add product...";
            this.EditProductDialog.Model = new Product();
            this.EditProductDialog.IsOpen = true;
        }

        private void OnDeactivateProductCommand(Product product)
        {
            try
            {
                WithClient(this._serviceFactory.CreateClient<IInventoryService>(), inventoryClient =>
                {
                    inventoryClient.DeleteProduct(product.ProductId);
                    product.IsActive = false;
                });
            }
            catch (FaultException ex)
            {
                ErrorOccured?.Invoke(this, new ErrorMessageEventArgs(ex.Message));
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(this, new ErrorMessageEventArgs(ex.Message));
            }
        }

        private void OnActivateProductCommand(Product product)
        {
            try
            {
                WithClient(this._serviceFactory.CreateClient<IInventoryService>(), inventoryClient =>
                {
                    inventoryClient.ActivateProduct(product.ProductId);
                    product.IsActive = true;
                });
            }
            catch (FaultException ex)
            {
                ErrorOccured?.Invoke(this, new ErrorMessageEventArgs(ex.Message));
            }
            catch (Exception ex)
            {
                ErrorOccured?.Invoke(this, new ErrorMessageEventArgs(ex.Message));
            }
        }

        #endregion
    }
}
