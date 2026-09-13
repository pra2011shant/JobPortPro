using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortPro.Models.Common;

namespace JobPortPro.Models
{
    /// <summary>
    /// Represents a saved/bookmarked job by a Job Seeker.
    /// </summary>
    public class SavedJob : BaseEntity
    {
        [Required]
        public int JobId { get; set; }
        [ForeignKey("JobId")]
        public virtual Job? Job { get; set; }

        [Required]
        public int JobSeekerId { get; set; }
        [ForeignKey("JobSeekerId")]
        public virtual User? JobSeeker { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
