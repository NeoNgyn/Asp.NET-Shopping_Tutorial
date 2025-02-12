using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models.Vnpay
{
    public class VnpayInfoModel
    {
        [Key]
        public int Id { get; set; }
        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public string Name { get; set; }
        public double Amount { get; set; }

        public DateTime DatePaid { get; set; }
    }
}
