using System.ComponentModel.DataAnnotations;

namespace Shopping_Tutorial.Models
{
    public class UserModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Please input username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Please input username"), EmailAddress]
        public string Email { get; set; }

        [DataType(DataType.Password), Required(ErrorMessage = "Please input password")]
        public string Password { get; set; }
    }
}
