using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JobPortPro.Data;
using JobPortPro.Models;
using JobPortPro.Models.Common;

namespace JobPortPro.Repositories
{
    /// <summary>
    /// Unit of Work implementation managing Entity Framework Core contexts & repositories.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public IRepository<T> Repository<T>() where T : BaseEntity
        {
            return (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_context));
        }

        public IRepository<User> Users => Repository<User>();
        public IRepository<Job> Jobs => Repository<Job>();
        public IRepository<JobApplication> JobApplications => Repository<JobApplication>();
        public IRepository<Category> Categories => Repository<Category>();
        public IRepository<JobType> JobTypes => Repository<JobType>();
        public IRepository<ExperienceLevel> ExperienceLevels => Repository<ExperienceLevel>();
        public IRepository<CompanyProfile> CompanyProfiles => Repository<CompanyProfile>();
        public IRepository<JobSeekerProfile> JobSeekerProfiles => Repository<JobSeekerProfile>();
        public IRepository<SavedJob> SavedJobs => Repository<SavedJob>();

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
