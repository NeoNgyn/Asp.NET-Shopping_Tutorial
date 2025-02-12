namespace Shopping_Tutorial.Models.ViewModels
{
    public class CartItemViewModel
    {
        public List<CartItemModel> CartItems { get; set; }

        public double GrandTotal { get; set; }


        public double ShippingCost { get; set; }

        public string CouponCode { get; set; }
    }
}
