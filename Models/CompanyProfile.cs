using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortPro.Models
{
    public class CompanyProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(150)]
        public string CompanyName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [StringLength(200)]
        [Url(ErrorMessage = "Please enter a valid website URL")]
        public string? Website { get; set; }

        [StringLength(150)]
        public string? Location { get; set; }

        public string? LogoUrl { get; set; }

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(50)]
        public string? CompanySize { get; set; }
    }
}
