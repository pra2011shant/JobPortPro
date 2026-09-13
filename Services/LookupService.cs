using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Implements database-driven lookups with in-memory caching for high-throughput, low-latency performance.
    /// </summary>
    public class LookupService : ILookupService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKeyCategories = "Lookup_Categories";
        private const string CacheKeyJobTypes = "Lookup_JobTypes";
        private const string CacheKeyExpLevels = "Lookup_ExperienceLevels";

        public LookupService(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Category>> GetCategoriesAsync(bool useCache = true)
        {
            if (useCache && _cache.TryGetValue(CacheKeyCategories, out List<Category>? cached) && cached != null)
            {
                return cached;
            }

            var categories = await _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive)
                .Include(c => c.Jobs.Where(j => j.IsActive))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            if (useCache)
            {
                _cache.Set(CacheKeyCategories, categories, TimeSpan.FromMinutes(15));
            }

            return categories;
        }

        public async Task<List<JobType>> GetJobTypesAsync(bool useCache = true)
        {
            if (useCache && _cache.TryGetValue(CacheKeyJobTypes, out List<JobType>? cached) && cached != null)
            {
                return cached;
            }

            var jobTypes = await _context.JobTypes
                .AsNoTracking()
                .Where(jt => jt.IsActive)
                .OrderBy(jt => jt.DisplayOrder)
                .ThenBy(jt => jt.Name)
                .ToListAsync();

            if (useCache)
            {
                _cache.Set(CacheKeyJobTypes, jobTypes, TimeSpan.FromMinutes(15));
            }

            return jobTypes;
        }

        public async Task<List<ExperienceLevel>> GetExperienceLevelsAsync(bool useCache = true)
        {
            if (useCache && _cache.TryGetValue(CacheKeyExpLevels, out List<ExperienceLevel>? cached) && cached != null)
            {
                return cached;
            }

            var expLevels = await _context.ExperienceLevels
                .AsNoTracking()
                .Where(el => el.IsActive)
                .OrderBy(el => el.DisplayOrder)
                .ThenBy(el => el.MinYears)
                .ToListAsync();

            if (useCache)
            {
                _cache.Set(CacheKeyExpLevels, expLevels, TimeSpan.FromMinutes(15));
            }

            return expLevels;
        }

        public async Task<List<string>> GetPopularLocationsAsync(int count = 15)
        {
            return await _context.Jobs
                .AsNoTracking()
                .Where(j => j.IsActive && !string.IsNullOrEmpty(j.Location))
                .Select(j => j.Location)
                .Distinct()
                .Take(count)
                .ToListAsync();
        }
    }
}
