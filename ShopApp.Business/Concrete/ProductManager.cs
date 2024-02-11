using ShopApp.Business.Abstract;
using ShopApp.DataAccess.Abstract;
using ShopApp.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopApp.Business.Concrete
{
    public class ProductManager : IProductService
    {
        private readonly IUnitOfWork _unitofwork;
        public ProductManager(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public void Create(Product entity)
        {
            _unitofwork.Products.Create(entity);
            _unitofwork.Save();

        }
        public bool Create(Product entity, int[] categoryIds)
        {
            if (Validation(entity))
            {
                if (categoryIds.Length == 0)
                {
                    ErrorMessage += "Ürün için en az 1 kategori seçmelisiniz.";
                    return false;
                }
                _unitofwork.Products.Create(entity);
                _unitofwork.Save();

                return true;

            }
            return false;


        }
        public async Task<Product> CreateAsync(Product entity)
        {
            await _unitofwork.Products.CreateAsync(entity);
            await _unitofwork.SaveAsync();
            return entity;
        }

        public void Delete(Product entity)
        {
            _unitofwork.Products.Delete(entity);
            _unitofwork.Save();

        }

        public async Task<List<Product>> GetAll()
        {
            return await _unitofwork.Products.GetAll();
        }

        public async Task<Product> GetById(int id)
        {
            return await _unitofwork.Products.GetById(id);
        }

        public Product GetByIdWithCategories(int id)
        {
            return _unitofwork.Products.GetByIdWithCategories(id);
        }

        public int GetCountByCategory(string category)
        {
            return (_unitofwork.Products.GetCountByCategory(category));
        }

        public List<Product> GetHomePageProducts()
        {
            return _unitofwork.Products.GetHomePageProducts();
        }

        public Product GetProductDetails(string url)
        {
            return _unitofwork.Products.GetProductDetails( url);
        }

        public List<Product> GetProductsByCategory(string name, int page,int pageSize)
        {
            return _unitofwork.Products.GetProductsByCategory(name, page, pageSize);
        }

        public List<Product> GetSearchResult(string searchString)
        {
            return _unitofwork.Products.GetSearchResult(searchString);
        }

        public void Update(Product entity)
        {
            _unitofwork.Products.Update(entity);
            _unitofwork.Save();

        }

        public bool Update(Product entity, int[] categoryIds)
        {
            if (Validation(entity))
            {
                
                if (categoryIds.Length == 0)
                {
                    ErrorMessage += "Ürün için en az 1 kategori seçmelisiniz.";
                    return false;
                }
                _unitofwork.Products.Update(entity, categoryIds);
                _unitofwork.Save();

                return true;
            }
            return false;
        }

        public string ErrorMessage { get; set ; }


        public bool Validation(Product entity)
        {
            var IsValid = true;

            if (string.IsNullOrEmpty(entity.Name))
            {
                ErrorMessage += "ürün ismi girmelisiniz. \n ";
                IsValid = false;
            }
                 
            if (entity.Price<0)
            {
                ErrorMessage += "ürün fiyatı negatif olamaz \n";
                IsValid = false;
            }
            return IsValid;
        }

        public async Task UpdateAsync(Product entityToUpdate, Product entity)
        {
            entityToUpdate.Name = entity.Name;
            entityToUpdate.Price = entity.Price;
            entityToUpdate.Description = entity.Description;
            entityToUpdate.ImageUrl = entity.ImageUrl;
            entityToUpdate.Url = entity.Url;
            entityToUpdate.IsApproved = entity.IsApproved;
            entityToUpdate.ProductCategories = entity.ProductCategories;
            entityToUpdate.IsHome = entity.IsHome;

            await _unitofwork.SaveAsync();
        }

        public async Task DeleteAsync(Product entity)
        {
            _unitofwork.Products.Delete(entity);
            await _unitofwork.SaveAsync();
        }
    }
}
