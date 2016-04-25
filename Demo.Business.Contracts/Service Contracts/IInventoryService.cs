using System;
using System.ServiceModel;
using Core.Common.Exceptions;
using Demo.Business.Entities;

namespace Demo.Business.Contracts
{
    [ServiceContract]
    public interface IInventoryService
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
    }
}
