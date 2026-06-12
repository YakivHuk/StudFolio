using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace StudFolio.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Прізвище є обов'язковим")]
        [StringLength(50)]
        public string Lastname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ім'я є обов'язковим")]
        [StringLength(50)]
        public string Firstname { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Middlename { get; set; }

        [Required(ErrorMessage = "Електронна пошта є обов'язковою")]
        [EmailAddress(ErrorMessage = "Некоректний формат пошти")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, введіть пароль")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль має бути не менше 8 символів.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Пароль повинен містити хоча б одну велику літеру, одну малу літеру та одну цифру.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Підтвердження пароля є обов'язковим")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролі не збігаються")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EducationInstitution { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        public IFormFile? AvatarFile { get; set; }
    }
}