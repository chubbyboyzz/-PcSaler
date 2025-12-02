using PcSaler.DBcontext.Entites;
using System.ComponentModel.DataAnnotations;

namespace PcSaler.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Enter username, please!")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Enter password, please!")]
        public string? Password { get; set; }
    }
}
