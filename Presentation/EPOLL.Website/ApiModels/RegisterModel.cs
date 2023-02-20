using System.ComponentModel.DataAnnotations;

namespace EPOLL.Website.ApiModels
{
    public class RegisterModel
    {
        // [EmailAddress(ErrorMessage = "Email is invalid")]
        [Required]
        public string Text { get; set; }

        [EmailAddress(ErrorMessage = "Email is invalid")]
        [Required]
        public string Email { get; set; }

        [Required]
        public string OrganizationName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 6)]
        public string Password { get; set; }
    }
}
