using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Core.Common.Exceptions;
using Demo.Client.Entities;
using Core.Common.Contracts;

namespace Demo.Client.Contracts
{
    [ServiceContract]
    public interface IInventoryService : IServiceContract
    {
        [OperationContract]
        Product[] GetProducts();

        [OperationContract]
        Product[] GetActiveProducts();

        [OperationContract]
        [FaultContract(typeof(NotFoundException))]
        Product GetProductById(int id, bool acceptNullable = false);

        [OperationContract]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        Product UpdateProduct(Product product);

        [OperationContract]
        [FaultContract(typeof(NotFoundException))]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        void DeleteProduct(int productId);

        [OperationContract]
        [FaultContract(typeof(NotFoundException))]
        [TransactionFlow(TransactionFlowOption.Allowed)]
        void ActivateProduct(int productId);

        [OperationContract]
        Product[] GetMostWanted(DateTime start, DateTime end);

        #region Async

        [OperationContract]
        Task<Product[]> GetProductsAsync();

        [OperationContract]
        Task<Product[]> GetActiveProductsAsync();

        [OperationContract]
        Task<Product> GetProductByIdAsync(int id, bool acceptNullable = false);

        [OperationContract]
        Task<Product> UpdateProductAsync(Product product);

        [OperationContract]
        Task DeleteProductAsync(int productId);

        [OperationContract]
        Task ActivateProductAsync(int productId);

        [OperationContract]
        Task<Product[]> GetMostWantedAsync(DateTime start, DateTime end);

        #endregion
    }
}
