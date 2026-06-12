using System.ComponentModel.DataAnnotations;

namespace StudFolio.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Не вказано прізвище")]
        [StringLength(50)]
        public string Lastname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Не вказано ім'я")]
        [StringLength(50)]
        public string Firstname { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Middlename { get; set; }

        [Required(ErrorMessage = "Некоректна адреса")]
        [EmailAddress(ErrorMessage = "Некоректна адреса е-пошти")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль є обов'язковим")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EducationInstitution { get; set; }

        [StringLength(100)]
        public string? Specialty { get; set; }

        public string? Avatar { get; set; }

        [Required]
        public string Role { get; set; } = "user";

        [Required]
        public DateTime CreationTime { get; set; } = DateTime.Now;

        public List<Post> Posts { get; set; } = new();

        public Portfolio? Portfolio { get; set; }
    }
}