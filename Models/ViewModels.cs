using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JobPortPro.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters", MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, ErrorMessage = "{0} must be at least {2} characters long", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an account type")]
        [Display(Name = "I am a")]
        public string Role { get; set; } = "JobSeeker";

        [Display(Name = "Company Name (Employers only)")]
        public string? CompanyName { get; set; }
    }

    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class JobFilterViewModel
    {
        public string? Query { get; set; }
        public int? CategoryId { get; set; }
        public string? JobType { get; set; }
        public string? Location { get; set; }
        public string? ExperienceLevel { get; set; }
        public decimal? MinSalary { get; set; }
        public string? SortBy { get; set; } = "newest";

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 9;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public List<Job> Jobs { get; set; } = new List<Job>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<JobType> DynamicJobTypes { get; set; } = new List<JobType>();
        public List<ExperienceLevel> DynamicExperienceLevels { get; set; } = new List<ExperienceLevel>();
        public List<string> Locations { get; set; } = new List<string>();
    }

    public class PostJobViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Job Title is required")]
        [Display(Name = "Job Title")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Job Category")]
        public int CategoryId { get; set; }

        public int? JobTypeId { get; set; }

        [Required(ErrorMessage = "Job Type is required")]
        [Display(Name = "Job Type")]
        public string JobType { get; set; } = "Full-Time";

        [Required(ErrorMessage = "Location is required")]
        [Display(Name = "Job Location")]
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Minimum Salary (Annual in ₹)")]
        public decimal? SalaryMin { get; set; }

        [Display(Name = "Maximum Salary (Annual in ₹)")]
        public decimal? SalaryMax { get; set; }

        public int? ExperienceLevelId { get; set; }

        [Display(Name = "Experience Level")]
        public string? ExperienceLevel { get; set; } = "Mid Level";

        [Required(ErrorMessage = "Job Description is required")]
        [Display(Name = "Job Description")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Key Requirements / Qualifications")]
        public string? Requirements { get; set; }

        [Display(Name = "Responsibilities")]
        public string? Responsibilities { get; set; }

        [Display(Name = "Application Deadline")]
        [DataType(DataType.Date)]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        public List<Category>? AvailableCategories { get; set; }
        public List<JobType>? AvailableJobTypes { get; set; }
        public List<ExperienceLevel>? AvailableExperienceLevels { get; set; }
    }

    public class ApplyJobViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;

        [Display(Name = "Cover Letter / Note to Employer")]
        public string? CoverLetter { get; set; }

        [Display(Name = "Resume File (PDF / DOC / DOCX)")]
        public IFormFile? ResumeFile { get; set; }

        public bool UseProfileResume { get; set; } = false;
        public string? ExistingResumeFileName { get; set; }
    }

    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string Role { get; set; } = "JobSeeker";

        [Required(ErrorMessage = "Full Name is required")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Bio / Summary")]
        public string? Bio { get; set; }

        public string? ProfilePicture { get; set; }
        public IFormFile? ProfilePictureFile { get; set; }

        // Job Seeker Specific Fields
        [Display(Name = "Professional Headline")]
        public string? Headline { get; set; }

        [Display(Name = "Key Skills (comma separated)")]
        public string? Skills { get; set; }

        [Display(Name = "Years of Experience")]
        public int? ExperienceYears { get; set; }

        [Display(Name = "Highest Education")]
        public string? Education { get; set; }

        [Url]
        [Display(Name = "GitHub Profile URL")]
        public string? GitHubUrl { get; set; }

        [Url]
        [Display(Name = "LinkedIn Profile URL")]
        public string? LinkedInUrl { get; set; }

        public string? ResumeFileName { get; set; }
        public string? ResumeFilePath { get; set; }
        public IFormFile? ResumeUpload { get; set; }

        // Employer Specific Fields
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [Display(Name = "Company Description")]
        public string? CompanyDescription { get; set; }

        [Url]
        [Display(Name = "Company Website")]
        public string? Website { get; set; }

        [Display(Name = "Company Location / HQ")]
        public string? CompanyLocation { get; set; }

        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Display(Name = "Company Size")]
        public string? CompanySize { get; set; }
    }

    public class EmployerDashboardViewModel
    {
        public int TotalJobsPosted { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalApplications { get; set; }
        public int ShortlistedApplications { get; set; }
        public List<Job> RecentJobs { get; set; } = new List<Job>();
        public List<JobApplication> RecentApplications { get; set; } = new List<JobApplication>();
    }

    public class JobSeekerDashboardViewModel
    {
        public int TotalApplications { get; set; }
        public int PendingApplications { get; set; }
        public int ShortlistedApplications { get; set; }
        public int SavedJobsCount { get; set; }
        public List<JobApplication> RecentApplications { get; set; } = new List<JobApplication>();
        public List<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
        public List<Job> RecommendedJobs { get; set; } = new List<Job>();
    }

    public class ApplicantDetailViewModel
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmail { get; set; } = string.Empty;
        public string? ApplicantPhone { get; set; }
        public string? Headline { get; set; }
        public string? Skills { get; set; }
        public int? ExperienceYears { get; set; }
        public string? Education { get; set; }
        public string? GitHubUrl { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? CoverLetter { get; set; }
        public string ResumePath { get; set; } = string.Empty;
        public string ResumeFileName { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; } = "Pending";
        public string? EmployerNotes { get; set; }
    }

    public class HomeIndexViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<JobType> JobTypes { get; set; } = new List<JobType>();
        public List<Job> FeaturedJobs { get; set; } = new List<Job>();
        public List<Job> RecentJobs { get; set; } = new List<Job>();
        public int TotalJobs { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalCandidates { get; set; }
        public int TotalApplications { get; set; }
    }
}
