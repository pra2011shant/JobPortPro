using System.Collections.Generic;
using System.Threading.Tasks;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Service for querying master data tables (Categories, Job Types, Experience Levels) dynamically from the database.
    /// </summary>
    public interface ILookupService
    {
        Task<List<Category>> GetCategoriesAsync(bool useCache = true);
        Task<List<JobType>> GetJobTypesAsync(bool useCache = true);
        Task<List<ExperienceLevel>> GetExperienceLevelsAsync(bool useCache = true);
        Task<List<string>> GetPopularLocationsAsync(int count = 15);
    }
}
