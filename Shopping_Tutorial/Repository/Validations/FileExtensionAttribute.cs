using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Repository.Validations
{
    public class FileExtensionAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if(value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                string[] extensionPaths = {"png", "jpg", "jpeg" };

                bool result = extensionPaths.Any(x => extension.EndsWith(x));

                if (!result)
                    return new ValidationResult("Allowed must be ended with png or jpg or jpeg");
            }

            return ValidationResult.Success;
        }
    }
}
