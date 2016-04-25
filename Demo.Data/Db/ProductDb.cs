using Demo.Business.Entities;
using System.Collections.Generic;

namespace Demo.Data.Db
{
    public static class ProductDb
    {
        public static List<Product> Get()
        {
            return new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ArticleNumber = "syrgsr",
                    Description = "Test product 1",
                    IsActive = true,
                    Name = "Product 1",
                    Price = 100
                },
                new Product
                {
                    ProductId = 2,
                    ArticleNumber = "vhjukjuh",
                    Description = "Test product 2",
                    IsActive = true,
                    Name = "Product 2",
                    Price = 200
                },
                new Product
                {
                    ProductId = 3,
                    ArticleNumber = "23545",
                    Description = "Test product 3",
                    IsActive = true,
                    Name = "Product 3",
                    Price = 300
                },
                new Product
                {
                    ProductId = 4,
                    ArticleNumber = "25545",
                    Description = "Test product 4",
                    IsActive = true,
                    Name = "Product 4",
                    Price = 400
                }
            };
        }
    }
}
