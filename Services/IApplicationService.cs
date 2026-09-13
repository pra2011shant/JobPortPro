using System.Collections.Generic;
using System.Threading.Tasks;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    public interface IApplicationService
    {
        Task<JobApplication> SubmitApplicationAsync(int seekerId, ApplyJobViewModel model, string resumePath, string resumeFileName);
        Task<bool> HasUserAppliedAsync(int seekerId, int jobId);
        Task<List<JobApplication>> GetApplicationsBySeekerAsync(int seekerId, string? status = null);
        Task<List<JobApplication>> GetApplicationsByEmployerAsync(int employerId, int? jobId = null, string? status = null);
        Task<JobApplication?> GetApplicationDetailAsync(int employerId, int applicationId);
        Task<bool> UpdateApplicationStatusAsync(int employerId, int applicationId, string status, string? notes);
        Task<bool> WithdrawApplicationAsync(int seekerId, int applicationId);
    }
}
