using Shopping_Tutorial.Repository.Validations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping_Tutorial.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }

        [Required, MinLength(1, ErrorMessage = "Category Name is Required")]
        public string Name { get; set; }

        [Required, MaxLength(1000000000, ErrorMessage = "Product Description is Required")]
        public string Description { get; set; }

        public string Slug { get; set; }

        [Required(ErrorMessage = "Product Price is Required")]
        public double Price { get; set; }

        [Required(ErrorMessage = "Product Price is Required")]
        public double CapitalPrice { get; set; }

        public string Image { get; set; } 

        [Required, Range(1, int.MaxValue, ErrorMessage = "Choose a category")]
        public int CategoryId { get; set; }

        public CategoryModel Category { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Choose a brand")]
        public int BrandId { get; set; }

        public BrandModel Brand { get; set; }


        public RatingModel Ratings { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

        public int? Quantity { get; set; }

        public int? SoldQuantity { get; set; }

    }
}
