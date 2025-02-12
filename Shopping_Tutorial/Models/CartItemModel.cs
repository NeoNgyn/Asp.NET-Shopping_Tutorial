namespace Shopping_Tutorial.Models
{
    public class CartItemModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public double Price { get; set; }

        public string Image { get; set; }

        public int Quantity { get; set; }

        public double Total
        {
            get => Quantity * Price;
        }

        public CartItemModel()
        {
            
        }

        public CartItemModel(ProductModel product)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Price = product.Price;
            Image = product.Image;
            Quantity = 1;
        }
    }
}
