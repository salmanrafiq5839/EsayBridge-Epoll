using System.ComponentModel.DataAnnotations;

namespace EPOLL.Website.ApiModels
{
    public class LoginModel
    {
        [EmailAddress(ErrorMessage = "Email is invalid")]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }

    public class ForgotModel
    {
        [EmailAddress(ErrorMessage = "Email is invalid")]
        [Required]
        public string Email { get; set; }
    }
}
