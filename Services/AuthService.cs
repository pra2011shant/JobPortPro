using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;

        public AuthService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (existingUser != null)
            {
                return (false, "An account with this email already exists.", null);
            }

            var user = new User
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLower(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role == "Employer" ? "Employer" : "JobSeeker",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (user.Role == "Employer")
            {
                var companyProfile = new CompanyProfile
                {
                    UserId = user.Id,
                    CompanyName = !string.IsNullOrWhiteSpace(model.CompanyName) ? model.CompanyName.Trim() : $"{model.FullName}'s Company",
                    Location = "Not Specified"
                };
                _context.CompanyProfiles.Add(companyProfile);
            }
            else
            {
                var seekerProfile = new JobSeekerProfile
                {
                    UserId = user.Id,
                    Headline = "Job Seeker"
                };
                _context.JobSeekerProfiles.Add(seekerProfile);
            }

            await _context.SaveChangesAsync();
            return (true, string.Empty, user);
        }

        public async Task<User?> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
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

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
