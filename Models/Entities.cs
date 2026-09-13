using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortPro.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Full Name is required")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "JobSeeker"; // "JobSeeker", "Employer", "Admin"

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        public string? ProfilePicture { get; set; }

        public string? Bio { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual CompanyProfile? CompanyProfile { get; set; }
        public virtual JobSeekerProfile? JobSeekerProfile { get; set; }
        public virtual ICollection<Job> PostedJobs { get; set; } = new List<Job>();
        public virtual ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
        public virtual ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    }

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

        public string? Skills { get; set; } // Comma-separated list

        public int? ExperienceYears { get; set; }

        public string? Education { get; set; }

        [StringLength(200)]
        [Url]
        public string? GitHubUrl { get; set; }

        [StringLength(200)]
        [Url]
        public string? LinkedInUrl { get; set; }
    }

    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(50)]
        public string? IconClass { get; set; } = "fa-briefcase";

        public string? Description { get; set; }

        public virtual ICollection<Job> Jobs { get; set; } = new List<Job>();
    }

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

    public class JobApplication
    {
        [Key]
        public int Id { get; set; }

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

    public class SavedJob
    {
        [Key]
        public int Id { get; set; }

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
