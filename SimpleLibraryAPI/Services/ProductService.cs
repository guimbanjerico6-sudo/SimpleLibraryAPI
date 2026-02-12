using SimpleLibraryAPI.Models;

namespace SimpleLibraryAPI.Services
{
    public class ProductService
    {
        private static List<Product> _products = new List<Product>();

        public List<Product> GetAllProducts() => _products;

        public Product GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

        public string AddProduct(Product newProduct)
        {
            if (_products.Any(p => p.Id == newProduct.Id))
                return "Duplicate";
            if (newProduct.Price < 0)
                return "InvalidPrice";
            _products.Add(newProduct);
            return "Success";
        }

        public bool UpdateProduct(int id, int newPrice)
        {
            var product = GetById(id);
            if (product == null) return false;
            product.Price = newPrice;
            return true;
        }
        public bool DeleteProduct(int id)
        {
            var product = GetById(id);
            if (product == null) return false;
            _products.Remove(product);
            return true;
        }

    }
}
