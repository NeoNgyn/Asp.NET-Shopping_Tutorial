using Shopping_Tutorial.Repository.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class SliderModel
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Slider Name is Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Slider Descriprion is Required")]
        public string Description { get; set; }

        public int Status { get; set; }

        public string Image { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

    }
}
