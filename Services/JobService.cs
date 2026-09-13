using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Handles business logic and data access for job listings, categories, and bookmarks.
    /// </summary>
    public class JobService : IJobService
    {
        private readonly ApplicationDbContext _context;

        public JobService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves paginated, filtered, and sorted job listings.
        /// </summary>
        public async Task<JobFilterViewModel> GetFilteredJobsAsync(
            string? query,
            int? categoryId,
            string? jobType,
            string? location,
            string? experienceLevel,
            decimal? minSalary,
            string? sortBy,
            int page,
            int pageSize)
        {
            var q = _context.Jobs
                .AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrWhiteSpace(query))
            {
                string searchLower = query.Trim().ToLower();
                q = q.Where(j =>
                    j.Title.ToLower().Contains(searchLower) ||
                    j.Description.ToLower().Contains(searchLower) ||
                    (j.Requirements != null && j.Requirements.ToLower().Contains(searchLower)) ||
                    (j.Employer != null && j.Employer.CompanyProfile != null && j.Employer.CompanyProfile.CompanyName.ToLower().Contains(searchLower)));
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                q = q.Where(j => j.CategoryId == categoryId.Value);
            }

            // Job type filter
            if (!string.IsNullOrWhiteSpace(jobType))
            {
                q = q.Where(j => j.JobType == jobType);
            }

            // Location filter
            if (!string.IsNullOrWhiteSpace(location))
            {
                q = q.Where(j => j.Location.ToLower().Contains(location.Trim().ToLower()));
            }

            // Experience level filter
            if (!string.IsNullOrWhiteSpace(experienceLevel))
            {
                q = q.Where(j => j.ExperienceLevel == experienceLevel);
            }

            // Salary filter
            if (minSalary.HasValue && minSalary.Value > 0)
            {
                q = q.Where(j => j.SalaryMax >= minSalary.Value || j.SalaryMin >= minSalary.Value);
            }

            // Sorting
            q = sortBy switch
            {
                "salary_high" => q.OrderByDescending(j => j.SalaryMax ?? j.SalaryMin ?? 0),
                "salary_low" => q.OrderBy(j => j.SalaryMin ?? j.SalaryMax ?? 0),
                _ => q.OrderByDescending(j => j.CreatedAt)
            };

            int totalItems = await q.CountAsync();
            var jobs = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .ToListAsync();

            var locations = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.IsActive && !string.IsNullOrEmpty(j.Location))
                .Select(j => j.Location)
                .Distinct()
                .Take(15)
                .ToListAsync();

            return new JobFilterViewModel
            {
                Query = query,
                CategoryId = categoryId,
                JobType = jobType,
                Location = location,
                ExperienceLevel = experienceLevel,
                MinSalary = minSalary,
                SortBy = sortBy ?? "newest",
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Jobs = jobs,
                Categories = categories,
                Locations = locations
            };
        }

        public async Task<Job?> GetJobByIdAsync(int id)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Include(j => j.Applications)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<List<Job>> GetJobsByEmployerAsync(int employerId, string? status = null)
        {
            var q = _context.Jobs
                .AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Applications)
                .Where(j => j.EmployerId == employerId);

            if (status == "active")
            {
                q = q.Where(j => j.IsActive);
            }
            else if (status == "inactive")
            {
                q = q.Where(j => !j.IsActive);
            }

            return await q.OrderByDescending(j => j.CreatedAt).ToListAsync();
        }

        public async Task<List<Job>> GetFeaturedJobsAsync(int count = 6)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Job>> GetRecentJobsAsync(int count = 4)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Category)
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Job>> GetRelatedJobsAsync(int categoryId, int excludeJobId, int count = 3)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Include(j => j.Employer)
                    .ThenInclude(e => e!.CompanyProfile)
                .Where(j => j.CategoryId == categoryId && j.Id != excludeJobId && j.IsActive)
                .OrderByDescending(j => j.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Include(c => c.Jobs.Where(j => j.IsActive))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Job> CreateJobAsync(int employerId, PostJobViewModel model)
        {
            var job = new Job
            {
                EmployerId = employerId,
                Title = model.Title.Trim(),
                CategoryId = model.CategoryId,
                JobType = model.JobType,
                Location = model.Location.Trim(),
                SalaryMin = model.SalaryMin,
                SalaryMax = model.SalaryMax,
                ExperienceLevel = model.ExperienceLevel,
                Description = model.Description.Trim(),
                Requirements = model.Requirements?.Trim(),
                Responsibilities = model.Responsibilities?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow,
                Deadline = model.Deadline
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<bool> UpdateJobAsync(int employerId, PostJobViewModel model)
        {
            if (!model.Id.HasValue) return false;

            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == model.Id.Value && j.EmployerId == employerId);
            if (job == null) return false;

            job.Title = model.Title.Trim();
            job.CategoryId = model.CategoryId;
            job.JobType = model.JobType;
            job.Location = model.Location.Trim();
            job.SalaryMin = model.SalaryMin;
            job.SalaryMax = model.SalaryMax;
            job.ExperienceLevel = model.ExperienceLevel;
            job.Description = model.Description.Trim();
            job.Requirements = model.Requirements?.Trim();
            job.Responsibilities = model.Responsibilities?.Trim();
            job.IsActive = model.IsActive;
            job.Deadline = model.Deadline;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleJobStatusAsync(int employerId, int jobId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);
            if (job == null) return false;

            job.IsActive = !job.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteJobAsync(int employerId, int jobId)
        {
            var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == jobId && j.EmployerId == employerId);
            if (job == null) return false;

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSavedJobAsync(int seekerId, int jobId)
        {
            var saved = await _context.SavedJobs.FirstOrDefaultAsync(s => s.JobId == jobId && s.JobSeekerId == seekerId);
            if (saved != null)
            {
                _context.SavedJobs.Remove(saved);
                await _context.SaveChangesAsync();
                return false; // Removed
            }
            else
            {
                _context.SavedJobs.Add(new SavedJob
                {
                    JobId = jobId,
                    JobSeekerId = seekerId,
                    SavedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                return true; // Added
            }
        }

        public async Task<List<SavedJob>> GetSavedJobsAsync(int seekerId)
        {
            return await _context.SavedJobs
                .AsNoTracking()
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Employer)
                        .ThenInclude(e => e!.CompanyProfile)
                .Include(s => s.Job)
                    .ThenInclude(j => j!.Category)
                .Where(s => s.JobSeekerId == seekerId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();
        }

        public async Task<bool> IsJobSavedAsync(int seekerId, int jobId)
        {
            return await _context.SavedJobs
                .AsNoTracking()
                .AnyAsync(s => s.JobId == jobId && s.JobSeekerId == seekerId);
        }
    }
}
