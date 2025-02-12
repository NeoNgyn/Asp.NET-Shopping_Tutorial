using Shopping_Tutorial.Repository.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class BrandModel
    {
        [Key]
        public int Id { get; set; }

        [Required (ErrorMessage = "Brand Name is Required")]
        public string Name { get; set; }

        [Required (ErrorMessage = "Brand Descriprion is Required")]
        public string Description { get; set; }

        public string Slug { get; set; }

        public int Status { get; set; }

       
    }
}
