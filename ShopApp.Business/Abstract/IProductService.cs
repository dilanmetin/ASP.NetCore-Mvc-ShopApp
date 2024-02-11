using ShopApp.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopApp.Business.Abstract
{
    public interface IProductService:IValidator<Product>
    {
        Task<Product> GetById(int id);
        Product GetByIdWithCategories(int id);
        Product GetProductDetails(string url);
        Task<List<Product>> GetAll();
        List<Product> GetHomePageProducts();
        List<Product> GetSearchResult(string searchString);
        List<Product> GetProductsByCategory(string name, int page, int pageSize);
        bool Create(Product entity, int[] categoryIds);
        Task<Product> CreateAsync(Product entity);
        void Update(Product entity);
        bool Update(Product entity, int[] categoryIds);
        void Delete(Product entity);
        Task DeleteAsync(Product entity);
        Task UpdateAsync(Product entityToUpdate,Product entity);
        int GetCountByCategory(string category);
    }
}
