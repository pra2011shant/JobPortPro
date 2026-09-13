using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    /// <summary>
    /// Authenticates users and manages security workflows via SQL Server Stored Procedures and BCrypt.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IStoredProcedureExecutor _spExecutor;
        private readonly ApplicationDbContext _context;

        public AuthService(IStoredProcedureExecutor spExecutor, ApplicationDbContext context)
        {
            _spExecutor = spExecutor;
            _context = context;
        }

        public async Task<(bool Success, string ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model)
        {
            var outputParam = new SqlParameter("@UserId", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var parameters = new[]
            {
                new SqlParameter("@FullName", model.FullName.Trim()),
                new SqlParameter("@Email", model.Email.Trim().ToLower()),
                new SqlParameter("@PasswordHash", passwordHash),
                new SqlParameter("@Role", model.Role == "Employer" ? "Employer" : "JobSeeker"),
                new SqlParameter("@PhoneNumber", DBNull.Value),
                new SqlParameter("@CompanyName", (object?)model.CompanyName?.Trim() ?? DBNull.Value),
                outputParam
            };

            await _spExecutor.ExecuteStoredProcedureNonQueryAsync("dbo.sp_CreateUser", parameters);

            int userId = outputParam.Value != DBNull.Value ? Convert.ToInt32(outputParam.Value) : -1;

            if (userId == -1)
            {
                return (false, "An account with this email address already exists.", null);
            }

            var user = await GetUserByIdAsync(userId);
            return (true, string.Empty, user);
        }

        public async Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            var parameters = new[] { new SqlParameter("@Email", email.Trim().ToLower()) };

            var user = await _spExecutor.ExecuteStoredProcedureSingleAsync(
                "dbo.sp_GetUserByEmail",
                parameters,
                reader => new User
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    FullName = reader.GetString(reader.GetOrdinal("FullName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                    Role = reader.GetString(reader.GetOrdinal("Role")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) ? null : reader.GetString(reader.GetOrdinal("Bio")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    CompanyProfile = reader.IsDBNull(reader.GetOrdinal("CompanyProfileId")) ? null : new CompanyProfile
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("CompanyProfileId")),
                        CompanyName = reader.IsDBNull(reader.GetOrdinal("CompanyName")) ? "Company" : reader.GetString(reader.GetOrdinal("CompanyName")),
                        Description = reader.IsDBNull(reader.GetOrdinal("CompanyDescription")) ? null : reader.GetString(reader.GetOrdinal("CompanyDescription")),
                        Website = reader.IsDBNull(reader.GetOrdinal("CompanyWebsite")) ? null : reader.GetString(reader.GetOrdinal("CompanyWebsite"))
                    },
                    JobSeekerProfile = reader.IsDBNull(reader.GetOrdinal("JobSeekerProfileId")) ? null : new JobSeekerProfile
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("JobSeekerProfileId")),
                        Headline = reader.IsDBNull(reader.GetOrdinal("SeekerHeadline")) ? "Candidate" : reader.GetString(reader.GetOrdinal("SeekerHeadline")),
                        Skills = reader.IsDBNull(reader.GetOrdinal("SeekerSkills")) ? null : reader.GetString(reader.GetOrdinal("SeekerSkills")),
                        ResumeFilePath = reader.IsDBNull(reader.GetOrdinal("SeekerResumePath")) ? null : reader.GetString(reader.GetOrdinal("SeekerResumePath"))
                    }
                });

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Include(u => u.CompanyProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> UpdateProfileAsync(int userId, ProfileViewModel model, string? avatarPath, string? resumePath, string? resumeFileName)
        {
            var user = await _context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return false;

            user.FullName = model.FullName.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();
            user.Bio = model.Bio?.Trim();

            if (!string.IsNullOrEmpty(avatarPath))
            {
                user.ProfilePicture = avatarPath;
            }

            if (user.Role == "JobSeeker")
            {
                if (user.JobSeekerProfile == null)
                {
                    user.JobSeekerProfile = new JobSeekerProfile { UserId = user.Id };
                    _context.JobSeekerProfiles.Add(user.JobSeekerProfile);
                }

                user.JobSeekerProfile.Headline = model.Headline?.Trim();
                user.JobSeekerProfile.Skills = model.Skills?.Trim();
                user.JobSeekerProfile.ExperienceYears = model.ExperienceYears;
                user.JobSeekerProfile.Education = model.Education?.Trim();
                user.JobSeekerProfile.GitHubUrl = model.GitHubUrl?.Trim();
                user.JobSeekerProfile.LinkedInUrl = model.LinkedInUrl?.Trim();

                if (!string.IsNullOrEmpty(resumePath))
                {
                    user.JobSeekerProfile.ResumeFilePath = resumePath;
                    user.JobSeekerProfile.ResumeFileName = resumeFileName;
                }
            }
            else if (user.Role == "Employer")
            {
                if (user.CompanyProfile == null)
                {
                    user.CompanyProfile = new CompanyProfile { UserId = user.Id };
                    _context.CompanyProfiles.Add(user.CompanyProfile);
                }

                user.CompanyProfile.CompanyName = model.CompanyName?.Trim() ?? $"{user.FullName}'s Company";
                user.CompanyProfile.Description = model.CompanyDescription?.Trim();
                user.CompanyProfile.Website = model.Website?.Trim();
                user.CompanyProfile.Location = model.CompanyLocation?.Trim();
                user.CompanyProfile.Industry = model.Industry?.Trim();
                user.CompanyProfile.CompanySize = model.CompanySize?.Trim();
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
