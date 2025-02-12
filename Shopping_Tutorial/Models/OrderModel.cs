namespace Shopping_Tutorial.Models
{
    public class OrderModel
    {
        public int Id { get; set; }

        public string OrderCode { get; set; }

        public string CouponCode { get; set; }

        public double ShippingCost { get; set; }

        public string PaymentMethod { get; set; }


        public string Username { get; set; }

        public string Address { get; set; }

        public DateTime CreatedDate { get; set; }

        public int Status { get; set; }
    }
}
