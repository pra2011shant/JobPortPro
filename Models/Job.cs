using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortPro.Models.Common;

namespace JobPortPro.Models
{
    /// <summary>
    /// Represents a published or draft Job Opening.
    /// </summary>
    public class Job : BaseEntity
    {
        [Required]
        public int EmployerId { get; set; }
        [ForeignKey("EmployerId")]
        public virtual User? Employer { get; set; }

        [Required(ErrorMessage = "Job Title is required")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        public int? JobTypeId { get; set; }
        [ForeignKey("JobTypeId")]
        public virtual JobType? JobTypeEntity { get; set; }

        [Required(ErrorMessage = "Job Type is required")]
        [StringLength(50)]
        public string JobType { get; set; } = "Full-Time";

        [Required(ErrorMessage = "Location is required")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMax { get; set; }

        public int? ExperienceLevelId { get; set; }
        [ForeignKey("ExperienceLevelId")]
        public virtual ExperienceLevel? ExperienceLevelEntity { get; set; }

        [StringLength(50)]
        public string? ExperienceLevel { get; set; } = "Mid Level";

        [Required(ErrorMessage = "Job Description is required")]
        public string Description { get; set; } = string.Empty;

        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? Deadline { get; set; }

        public int ViewsCount { get; set; } = 0;

        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public virtual ICollection<SavedJob> SavedByUsers { get; set; } = new List<SavedJob>();
    }
}
