using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SimpleLibraryAPI.Models;
using SimpleLibraryAPI.Services;

namespace SimpleLibraryAPI.Controllers
{
    [Route("api/[controller]")] // Sets the URL to: api/products
    [ApiController]             // Tells ASP.NET this class handles API requests
    public class ProductsController : ControllerBase
    {
        // Reference to our Service (The "Kitchen")
        private readonly ProductService _productService;

        // Constructor: The API gives the Controller the Service it needs to work
        public ProductsController(ProductService productService)
        {
            _productService = productService;
        }

        // GET: api/products
        [HttpGet]
        public List<Product> Get() => _productService.GetAllProducts();

        // GET: api/products/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // Ask the service to find the product
            var product = _productService.GetById(id);

            // If the product doesn't exist, tell the user with a 404 code
            if (product == null)
            {
                return NotFound($"Product with ID {id} was not found.");
            }

            // If found, send it back with a 200 OK code
            return Ok(product);
        }

        // POST: api/products
        [HttpPost]
        public IActionResult Post(Product newProduct)
        {
            // Send the data to the service and catch the "string" result
            var result = _productService.AddProduct(newProduct);

            // Switch statement to decide which "Status Code" to send back
            return result switch
            {
                "Duplicate" => Conflict("This ID already exists."), // Code 409
                "InvalidPrice" => BadRequest("Price cannot be negative."), // Code 400
                // Default: Success! Send 201 Created and tell the user where to find it
                _ => CreatedAtAction(nameof(Get), new { id = newProduct.Id }, newProduct)
            };
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, int newPrice)
        {
            // FIRST DEFENSE: Check if the input is valid before doing anything else
            if (newPrice <= 0)
            {
                return BadRequest("Price cannot be negative."); // Code 400
            }

            // Ask the service to perform the update
            var success = _productService.UpdateProduct(id, newPrice);

            // If the service says "false", it means the ID didn't exist
            if (!success)
            {
                return NotFound($"Product with ID {id} not found."); // Code 404
            }

            // Success: Return 200 OK
            return Ok("Price updated!");
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Ask the service to remove the product
            var success = _productService.DeleteProduct(id);

            // If it couldn't be found, return 404
            if (!success)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Success: Return 200 OK
            return Ok("Product removed.");
        }
    }
}