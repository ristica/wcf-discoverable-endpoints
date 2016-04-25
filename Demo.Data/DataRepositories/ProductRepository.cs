using System.ComponentModel.Composition;
using System.Linq;
using Demo.Business.Entities;
using Demo.Data.Contracts;
using Demo.Data.Db;

namespace Demo.Data.DataRepositories
{
    [Export(typeof(IProductRepository))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ProductRepository : IProductRepository
    {
        #region IProductRepository implementation

        public Product GetProductByArticleNumber(string articleNumber)
        {
            return ProductDb.Get().FirstOrDefault(p => p.ArticleNumber.Equals(articleNumber));
        }

        public Product GetProductById(int productId)
        {
            return ProductDb.Get().FirstOrDefault(p => p.ProductId == productId);
        }

        public Product[] GetActiveProducts()
        {
            return ProductDb.Get().Where(p => p.IsActive).ToArray();
        }

        public Product[] GetProducts()
        {
            return ProductDb.Get().ToArray();
        }

        public void ActivateProduct(int productId)
        {
            ProductDb.Get().FirstOrDefault(p => p.IsActive).IsActive = true;
        }

        public void DeactivateProduct(int productId)
        {
            ProductDb.Get().FirstOrDefault(p => p.IsActive).IsActive = false;
        }

        public Product UpdateProduct(Product product)
        {
            var p = ProductDb.Get().FirstOrDefault(x => x.ProductId == product.ProductId);

            if (p == null)
            {
                p = new Product
                {
                    ArticleNumber = product.ArticleNumber,
                    Description = product.Description,
                    IsActive = product.IsActive,
                    Name = product.Name,
                    Price = product.Price
                };
            }
            else
            {
                p.IsActive = product.IsActive;
                p.Name = product.Name;
                p.Price = product.Price;
                p.ArticleNumber = product.ArticleNumber;
                p.Description = product.Description;
            }           

            return p;
        }

        #endregion
    }
}
