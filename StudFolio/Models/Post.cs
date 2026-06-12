using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudFolio.Models
{
    public class Post
    {
        [Key]
        public int PostID { get; set; }

        [Required(ErrorMessage = "Будь ласка, введіть заголовок посту")]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, введіть опис")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Будь ласка, виберіть тип посту")]
        public string Type { get; set; } = string.Empty;

        public List<string> TechnologiesAndTools { get; set; } = new();
        public List<string> Links { get; set; } = new();
        public List<string> EmbeddedLinks { get; set; } = new();

        public string Preview { get; set; } = "/images/default-post-preview.jpg";

        public DateTime TimeOfPublication { get; set; } = DateTime.Now;

        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }
    }
}