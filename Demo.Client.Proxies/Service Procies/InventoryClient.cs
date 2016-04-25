using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Core.Common.ServiceModel;
using Demo.Client.Contracts;
using Demo.Client.Entities;

namespace Demo.Client.Proxies.Service_Procies
{
    [Export(typeof(IInventoryService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class InventoryClient : UserClientBase<IInventoryService>, IInventoryService
    {
        /// <summary>
        /// use this as importing c-tor when
        /// no dynamically endpoint discovery used and for unit tests!
        /// use [ImportingConstructor] if using hardcoded endpoint
        /// </summary>
        [ImportingConstructor]
        public InventoryClient()
        {

        }

        /// <summary>
        /// if using dynamic endpoint discovery
        /// use [ImportingConstructor] if using discovering the service
        /// </summary>
        /// <param name="endpointName"></param>
        //[ImportingConstructor]
        public InventoryClient([Import("Dynamic.Endpoint")] string endpointName) 
            : base(endpointName)
        {

        }

        #region IInventoryService implementation

        public Product[] GetProducts()
        {
            return Channel.GetProducts();
        }

        public Product[] GetActiveProducts()
        {
            return Channel.GetActiveProducts();
        }

        public Product GetProductById(int id, bool acceptNullable = false)
        {
            return Channel.GetProductById(id, acceptNullable);
        }

        public Product UpdateProduct(Product product)
        {
            return Channel.UpdateProduct(product);
        }

        public void DeleteProduct(int productId)
        {
            Channel.DeleteProduct(productId);
        }

        public void ActivateProduct(int productId)
        {
            Channel.ActivateProduct(productId);
        }

        public Product[] GetMostWanted(DateTime start, DateTime end)
        {
            return Channel.GetMostWanted(start, end);
        }

        public Task<Product[]> GetProductsAsync()
        {
            return Channel.GetProductsAsync();
        }

        public Task<Product[]> GetActiveProductsAsync()
        {
            return Channel.GetActiveProductsAsync();
        }

        public Task<Product> GetProductByIdAsync(int id, bool acceptNullable = false)
        {
            return Channel.GetProductByIdAsync(id, acceptNullable);
        }

        public Task<Product> UpdateProductAsync(Product product)
        {
            return Channel.UpdateProductAsync(product);
        }

        public Task DeleteProductAsync(int productId)
        {
            return Channel.DeleteProductAsync(productId);
        }

        public Task ActivateProductAsync(int productId)
        {
            return Channel.ActivateProductAsync(productId);
        }

        public Task<Product[]> GetMostWantedAsync(DateTime start, DateTime end)
        {
            return Channel.GetMostWantedAsync(start, end);
        }

        #endregion
    }
}
