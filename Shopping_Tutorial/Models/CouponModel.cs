using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class CouponModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name can be empty")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Description can be empty")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Quantity can be empty")]
        public int Quantity { get; set; }

        public DateTime DateStart { get; set; }

        public DateTime DateEnd { get; set; }

        public int Status { get; set; }
    }
}
