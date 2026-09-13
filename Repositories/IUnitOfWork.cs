using System;
using System.Threading.Tasks;
using JobPortPro.Models;
using JobPortPro.Models.Common;

namespace JobPortPro.Repositories
{
    /// <summary>
    /// Unit of Work pattern interface coordinating transactions across repositories (OOPs - Transaction Management).
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> Repository<T>() where T : BaseEntity;
        IRepository<User> Users { get; }
        IRepository<Job> Jobs { get; }
        IRepository<JobApplication> JobApplications { get; }
        IRepository<Category> Categories { get; }
        IRepository<JobType> JobTypes { get; }
        IRepository<ExperienceLevel> ExperienceLevels { get; }
        IRepository<CompanyProfile> CompanyProfiles { get; }
        IRepository<JobSeekerProfile> JobSeekerProfiles { get; }
        IRepository<SavedJob> SavedJobs { get; }

        Task<int> CompleteAsync();
    }
}
