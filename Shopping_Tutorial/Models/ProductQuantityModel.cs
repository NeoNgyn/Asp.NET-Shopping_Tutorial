using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class ProductQuantityModel
    {
        [Key]
        public int Id { get; set; }

        [Required (ErrorMessage = "Quantity is required")]
        public int Quantity { get; set; }

        public int ProductId { get; set; }

        public DateTime CreatedDate { get; set; }

        
    }
}
