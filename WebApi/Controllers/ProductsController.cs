using Microsoft.AspNetCore.Mvc;
using ShopApp.Business.Abstract;
using ShopApp.Entity;
using WebApi.DTO;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private IProductService _productService;
        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }
        [HttpGet]
        public async Task<IActionResult> GetProducts() 
        
        {
            var products =await _productService.GetAll();

            var productsDTO = new List<ProductDTO>();
            foreach (var p in products)
            {
                productsDTO.Add(ProductToDTO(p));
            }
            return Ok(productsDTO);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var p= await _productService.GetById(id);
            if(p == null)
            {
                return NotFound(); //404
            }
            return Ok(ProductToDTO(p)); //200
           
        }
        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product entity)
        {
            await _productService.CreateAsync(entity);
            return CreatedAtAction(nameof(GetProduct), new {id=entity.ProductId}, ProductToDTO(entity));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id , Product entity)
        {
            if(id!=entity.ProductId)
            {
                return BadRequest();
            }
            var product = await _productService.GetById(id);
            if(product == null)
            {
                return NotFound();
            }
            await _productService.UpdateAsync(product,entity);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productService.GetById(id);
            if(product == null) {
            return NotFound();
            }
            await _productService.DeleteAsync(product);
            return NoContent();
        }

        private static ProductDTO ProductToDTO(Product p)
        {
            return new ProductDTO
            {
                ProductId = p.ProductId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Url = p.Url
            };
        }

    }
}
