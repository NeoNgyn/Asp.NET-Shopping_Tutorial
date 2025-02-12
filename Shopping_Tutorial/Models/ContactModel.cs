using Shopping_Tutorial.Repository.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class ContactModel
    {
        [Key]
        public int Id { get; set; }

        [Required( ErrorMessage = "Conact Name is Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Conact Map is Required")]
        public string Map { get; set; }

        [Required(ErrorMessage = "Conact Email is Required")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Conact Phone is Required")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Contact Description is Required")]
        public string Description { get; set; }

        public string LogoImage { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
