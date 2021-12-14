using AutoMapper;
using ProductShop.DataTransferObject;
using ProductShop.Models;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            CreateMap<UserInputModel,User>();

            CreateMap<ProductInputModel, Product>();

            CreateMap<CategoryInputModel, Category>();

            CreateMap<CategoryProductModel, CategoryProductModel>();
        }
    }
}
