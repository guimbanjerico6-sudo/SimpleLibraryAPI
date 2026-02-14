using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Services
{
    public class ProductService
    {
        // STATIC LIST: This is our temporary "database" in RAM. 
        // It's static so the data stays there as long as the app is running.
        private static List<Product> _products = new List<Product>();

        // Return the whole list to the Controller
        // get all products
        public List<Product> GetAllProducts() => _products;

        // Search the list for a specific ID. Returns 'null' if not found.
        //get product by id
        public Product GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

        // Logic for adding a new product
        //add new product
        public string AddProduct(Product newProduct)
        {
            // Check if ID is already taken
            if (_products.Any(p => p.Id == newProduct.Id))
                return "Duplicate";

            // Check if price is negative (The service should always protect its data)
            if (newProduct.Price <= 0)
                return "InvalidPrice";

            // If all checks pass, add to the list
            _products.Add(newProduct);
            return "Success";
        }

        // Logic for updating an existing product
        // update product price
        public bool UpdateProduct(int id, int newPrice)
        {
            // Use our GetById method to find the item
            var product = GetById(id);

            // If it's not there, return false immediately
            if (product == null) return false;

            // Security check: Don't allow bad data into our list
            if (newPrice <= 0) return false;

            // Update the object in the list
            product.Price = newPrice;
            return true;
        }

        // Logic for removing a product
        // delete product
        public bool DeleteProduct(int id)
        {
            var product = GetById(id);

            if (product == null) return false;

            // Remove from the list
            _products.Remove(product);
            return true;
        }
    }
}