using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortPro.Models
{
    public class Job
    {
        [Key]
        public int Id { get; set; }

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

        [Required(ErrorMessage = "Job Type is required")]
        [StringLength(50)]
        public string JobType { get; set; } = "Full-Time"; // Full-Time, Part-Time, Remote, Contract, Internship

        [Required(ErrorMessage = "Location is required")]
        [StringLength(150)]
        public string Location { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMin { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? SalaryMax { get; set; }

        [StringLength(50)]
        public string? ExperienceLevel { get; set; } = "Mid Level"; // Entry Level, Mid Level, Senior Level, Director

        [Required(ErrorMessage = "Job Description is required")]
        public string Description { get; set; } = string.Empty;

        public string? Requirements { get; set; }
        public string? Responsibilities { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? Deadline { get; set; }

        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public virtual ICollection<SavedJob> SavedByUsers { get; set; } = new List<SavedJob>();
    }
}
