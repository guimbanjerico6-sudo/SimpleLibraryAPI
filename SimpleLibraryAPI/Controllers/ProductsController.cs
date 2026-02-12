using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleLibraryAPI.Models;
using SimpleLibraryAPI.Services;

namespace SimpleLibraryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public List<Models.Product> Get() => _productService.GetAllProducts();

        [HttpGet("{id}")]
        public Product Get(int id) => _productService.GetById(id);

        [HttpPost]
        public string Post(Product newProduct)
        {
            var result = _productService.AddProduct(newProduct);
            return result switch
            {
                "Duplicate" => $"Sorry, product with ID '{newProduct.Id}' already exists.",
                "InvalidPrice" => "Price cannot be negative.",
                _ => $"Success! Product '{newProduct.Name}' added."
            };
        }
        [HttpPut("{id}")]
        public string Put(int id, int newPrice)
        {
            var success = _productService.UpdateProduct(id, newPrice);
            return success ? "Price updated!" : $"Product with ID '{id}' not found.";
        }

        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            var success = _productService.DeleteProduct(id);
            return success ? "Product removed." : $"Product with ID '{id}' not found.";
        }
    }
}
