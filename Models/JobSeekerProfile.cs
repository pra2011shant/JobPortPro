using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortPro.Models
{
    public class JobSeekerProfile
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [StringLength(200)]
        public string? Headline { get; set; }

        public string? ResumeFileName { get; set; }
        public string? ResumeFilePath { get; set; }

        public string? Skills { get; set; }

        public int? ExperienceYears { get; set; }

        public string? Education { get; set; }

        [StringLength(200)]
        public string? GitHubUrl { get; set; }

        [StringLength(200)]
        public string? LinkedInUrl { get; set; }
    }
}
