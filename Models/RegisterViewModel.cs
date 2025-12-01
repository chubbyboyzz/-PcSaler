using System.ComponentModel.DataAnnotations;

namespace PcSaler.Models
{
    public class RegisterViewModel
    {
        // 1. USERNAME: No special chars, no spaces, only letters and numbers
        [Required(ErrorMessage = "Username cannot be empty")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "Username can only contain letters and numbers (no spaces or special characters)")]
        public string? Username { get; set; }

        // 2. FULL NAME: No numbers, allows unicode characters (for Vietnamese names)
        [Required(ErrorMessage = "Full name cannot be empty")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Full name cannot contain numbers or special characters")]
        public string? FullName { get; set; }

        // 3. PASSWORD: Strong password policy (At least 1 Upper, 1 Lower, 1 Number, 1 Special char)
        [Required(ErrorMessage = "Password cannot be empty")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one number, and one special character (@$!%*?&)")]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }

        // 4. EMAIL: Standard email format validation
        [Required(ErrorMessage = "Email cannot be empty")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? Email { get; set; }
    }
}