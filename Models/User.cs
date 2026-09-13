using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
}
