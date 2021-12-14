using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.DataTransferObject;
using ProductShop.Models;
using Microsoft.EntityFrameworkCore;
namespace ProductShop
{
    
    public class StartUp
    {
        static IMapper mapper;
        public static void Main(string[] args)
        {
            var productShopContext = new ProductShopContext();

            /*productShopContext.Database.EnsureDeleted();
            productShopContext.Database.EnsureCreated();



            string usersJson = File.ReadAllText("../../../Datasets/users.json");
            string productJson = File.ReadAllText("../../../Datasets/products.json");
            string categoryJson = File.ReadAllText("../../../Datasets/categories.json");
            string categoryProductJson = File.ReadAllText("../../../Datasets/categories-products.json");
            ImportUsers(productShopContext, usersJson);
            ImportProducts(productShopContext, productJson);
            ImportCategories(productShopContext, categoryJson);*/
            
            var result = GetUsersWithProducts(productShopContext);

            Console.WriteLine(result);
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {

            InitliazeAutoMapper();
            var dtoUsers = JsonConvert.DeserializeObject<IEnumerable<UserInputModel>>(inputJson);

            var users = mapper.Map<IEnumerable<User>>(dtoUsers);
            context.AddRange(users);
            context.SaveChanges();

            return $"Successfully imported {users.Count()}";
        }
        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            InitliazeAutoMapper();
            var dtoProduct = JsonConvert.DeserializeObject<IEnumerable<ProductInputModel>>(inputJson);

            var products = mapper.Map<IEnumerable<Product>>(dtoProduct);
            context.AddRange(products);
            context.SaveChanges();

            return $"Successfully imported {products.Count()}";
        }
        
        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            InitliazeAutoMapper();
            var dtoCategory = JsonConvert.DeserializeObject<IEnumerable<Category>>(inputJson)
                .Where(x => x.Name != null)
                .ToList();

            var categories = mapper.Map<IEnumerable<Category>>(dtoCategory);
            context.AddRange(categories);
            context.SaveChanges();

            return $"Successfully imported {categories.Count()}";
        }
        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            InitliazeAutoMapper();
            var dtoCategoryProduct = JsonConvert.DeserializeObject<IEnumerable<CategoryProduct>>(inputJson);
                

            var categoriesProduct = mapper.Map<IEnumerable<CategoryProduct>>(dtoCategoryProduct);
            context.AddRange(categoriesProduct);
            context.SaveChanges();

            return $"Successfully imported {categoriesProduct.Count()}";
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var product = context.Products
                .Where(x => x.Price >= 500 && x.Price <= 1000)
                .Select(x => new
                {
                    name = x.Name,
                    price = x.Price,
                    seller = x.Seller.FirstName + " " + x.Seller.LastName
                })
                .OrderBy(x => x.price)
                .ToArray();

            var result = JsonConvert.SerializeObject(product,Formatting.Indented);

            return result;
        }
        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context.Users
                .Where(x => x.ProductsSold.Any(p => p.Buyer != null))
                .Select(user => new
                {
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    soldProducts = user.ProductsSold.Where(sp => sp.Buyer != null).Select(p => new
                    {
                        name = p.Name,
                        price = p.Price,
                        buyerFirstName = p.Buyer.FirstName,
                        buyerLastName = p.Buyer.LastName
                    })
                    .ToArray()
                })
                .OrderBy(x => x.lastName)
                .ThenBy(x => x.firstName)
                .ToArray();


            var result = JsonConvert.SerializeObject(users, Formatting.Indented);

            return result;
        }
        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var categoryInfo = context.Categories
                .Select(x => new
                {
                    category = x.Name,
                    productsCount = x.CategoryProducts.Count,
                    averagePrice = x.CategoryProducts.Count == 0 ? 0.ToString("F2") : x.CategoryProducts.Average(p => p.Product.Price).ToString("F2"),
                    totalRevenue = x.CategoryProducts.Sum(r => r.Product.Price).ToString("F2")
                })
                .OrderByDescending(x => x.productsCount)
                .ToArray();

            var result = JsonConvert.SerializeObject(categoryInfo, Formatting.Indented);

            return result;

        }
        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Include(x => x.ProductsSold)
                .ToList()
                .Where(x => x.ProductsSold.Any(b => b.BuyerId != null))
                .Select(u => new
                {
                    firstname = u.FirstName,
                    lastname = u.LastName,
                    age = u.Age,
                    soldProduct = new
                    {
                        count = u.ProductsSold.Where(x => x.BuyerId != null).Count(),
                        product = u.ProductsSold.Where(x => x.BuyerId != null).Select(p => new
                        {
                            name = p.Name,
                            price = p.Price
                        })
                    }
                })
                .OrderByDescending(x => x.soldProduct.product.Count())
                .ToList();

            var resultObject = new
            {
                usersCount = users.Count(),
                users = users
            };

            var jsonSerelizer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var resultJson = JsonConvert.SerializeObject(resultObject, Formatting.Indented,jsonSerelizer);

            return resultJson;

        }

        private static void InitliazeAutoMapper()
        {
            var conf = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });
            mapper = conf.CreateMapper();
        }
    }
}