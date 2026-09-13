using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Implements database lookups via SQL Server Stored Procedures with in-memory caching.
    /// </summary>
    public class LookupService : ILookupService
    {
        private readonly IStoredProcedureExecutor _spExecutor;
        private readonly IMemoryCache _cache;
        private const string CacheKeyCategories = "Lookup_Categories";
        private const string CacheKeyJobTypes = "Lookup_JobTypes";
        private const string CacheKeyExpLevels = "Lookup_ExperienceLevels";

        public LookupService(IStoredProcedureExecutor spExecutor, IMemoryCache cache)
        {
            _spExecutor = spExecutor;
            _cache = cache;
        }

        public async Task<List<Category>> GetCategoriesAsync(bool useCache = true)
        {
            if (useCache && _cache.TryGetValue(CacheKeyCategories, out List<Category>? cached) && cached != null)
            {
                return cached;
            }

            var categories = await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetCategories", 
                null, 
                reader => new Category
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    IconClass = reader.IsDBNull(reader.GetOrdinal("IconClass")) ? "fa-solid fa-briefcase" : reader.GetString(reader.GetOrdinal("IconClass")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    Jobs = new List<Job>(new Job[reader.GetInt32(reader.GetOrdinal("ActiveJobsCount"))])
                });

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

            var jobTypes = await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetJobTypes",
                null,
                reader => new JobType
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    BadgeClass = reader.IsDBNull(reader.GetOrdinal("BadgeClass")) ? "badge-soft-primary" : reader.GetString(reader.GetOrdinal("BadgeClass")),
                    DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });

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

            var expLevels = await _spExecutor.ExecuteStoredProcedureListAsync(
                "dbo.sp_GetExperienceLevels",
                null,
                reader => new ExperienceLevel
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    MinYears = reader.GetInt32(reader.GetOrdinal("MinYears")),
                    MaxYears = reader.IsDBNull(reader.GetOrdinal("MaxYears")) ? null : reader.GetInt32(reader.GetOrdinal("MaxYears")),
                    DisplayOrder = reader.GetInt32(reader.GetOrdinal("DisplayOrder")),
                    IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt"))
                });

            if (useCache)
            {
                _cache.Set(CacheKeyExpLevels, expLevels, TimeSpan.FromMinutes(15));
            }

            return expLevels;
        }

        public async Task<List<string>> GetPopularLocationsAsync(int count = 15)
        {
            return await Task.FromResult(new List<string>
            {
                "Bengaluru, Karnataka",
                "Remote",
                "Mumbai, Maharashtra",
                "Pune, Maharashtra",
                "Hyderabad, Telangana",
                "Delhi NCR",
                "Chennai, Tamil Nadu"
            });
        }
    }
}
