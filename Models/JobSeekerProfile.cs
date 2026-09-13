using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortPro.Models.Common;

namespace JobPortPro.Models
{
    /// <summary>
    /// Represents a Job Seeker's professional skills, resume, and portfolio profile.
    /// </summary>
    public class JobSeekerProfile : BaseEntity
    {
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
