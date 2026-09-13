using System.Collections.Generic;
using System.Threading.Tasks;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    public interface IJobService
    {
        Task<JobFilterViewModel> GetFilteredJobsAsync(
            string? query,
            int? categoryId,
            string? jobType,
            string? location,
            string? experienceLevel,
            decimal? minSalary,
            string? sortBy,
            int page,
            int pageSize);

        Task<Job?> GetJobByIdAsync(int id);
        Task<List<Job>> GetJobsByEmployerAsync(int employerId, string? status = null);
        Task<List<Job>> GetFeaturedJobsAsync(int count = 6);
        Task<List<Job>> GetRecentJobsAsync(int count = 4);
        Task<List<Job>> GetRelatedJobsAsync(int categoryId, int excludeJobId, int count = 3);
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Job> CreateJobAsync(int employerId, PostJobViewModel model);
        Task<bool> UpdateJobAsync(int employerId, PostJobViewModel model);
        Task<bool> ToggleJobStatusAsync(int employerId, int jobId);
        Task<bool> DeleteJobAsync(int employerId, int jobId);
        Task<bool> ToggleSavedJobAsync(int seekerId, int jobId);
        Task<List<SavedJob>> GetSavedJobsAsync(int seekerId);
        Task<bool> IsJobSavedAsync(int seekerId, int jobId);
    }
}
