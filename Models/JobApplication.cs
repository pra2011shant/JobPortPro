using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortPro.Models.Common;

namespace JobPortPro.Models
{
    /// <summary>
    /// Represents a candidate's formal application for a specific Job Opening.
    /// </summary>
    public class JobApplication : BaseEntity
    {
        [Required]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public virtual Job? Job { get; set; }

        [Required]
        public int JobSeekerId { get; set; }
        [ForeignKey("JobSeekerId")]
        public virtual User? JobSeeker { get; set; }

        public string? CoverLetter { get; set; }

        [Required]
        public string ResumePath { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string ResumeFileName { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Shortlisted, Rejected, Accepted

        public string? EmployerNotes { get; set; }
    }
}
