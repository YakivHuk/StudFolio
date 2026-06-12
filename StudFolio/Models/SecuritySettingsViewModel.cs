using System.ComponentModel.DataAnnotations;

namespace StudFolio.Models
{
    public class SecuritySettingsViewModel
    {
        [Required(ErrorMessage = "Будь ласка, введіть поточний пароль")]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, введіть новий пароль")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Пароль повинен містити щонайменше 8 символів, одну малу літеру, одну велику літеру та одну цифру")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, повторіть новий пароль")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Паролі не збігаються")]
        public string AgainPassword { get; set; } = string.Empty;

        public string? StatusMessage { get; set; }
    }
}