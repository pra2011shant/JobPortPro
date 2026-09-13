using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobPortPro.Data;
using JobPortPro.Models;

namespace JobPortPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? role)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterViewModel
            {
                Role = (role == "Employer") ? "Employer" : "JobSeeker"
            };
            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "An account with this email already exists.");
                    return View(model);
                }

                // Create user
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

                // Create profile based on role
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

                // Sign in user
                await SignInUserAsync(user, false);

                TempData["SuccessMessage"] = $"Welcome to JobPortPro, {user.FullName}! Your account has been created.";

                if (user.Role == "Employer")
                {
                    return RedirectToAction("Dashboard", "Employer");
                }
                else
                {
                    return RedirectToAction("Dashboard", "JobSeeker");
                }
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .Include(u => u.CompanyProfile)
                    .Include(u => u.JobSeekerProfile)
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    await SignInUserAsync(user, model.RememberMe);
                    TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    if (user.Role == "Employer")
                    {
                        return RedirectToAction("Dashboard", "Employer");
                    }
                    else
                    {
                        return RedirectToAction("Dashboard", "JobSeeker");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password.");
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been successfully logged out.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new ProfileViewModel
            {
                UserId = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Bio = user.Bio,
                ProfilePicture = user.ProfilePicture
            };

            if (user.Role == "JobSeeker" && user.JobSeekerProfile != null)
            {
                model.Headline = user.JobSeekerProfile.Headline;
                model.Skills = user.JobSeekerProfile.Skills;
                model.ExperienceYears = user.JobSeekerProfile.ExperienceYears;
                model.Education = user.JobSeekerProfile.Education;
                model.GitHubUrl = user.JobSeekerProfile.GitHubUrl;
                model.LinkedInUrl = user.JobSeekerProfile.LinkedInUrl;
                model.ResumeFileName = user.JobSeekerProfile.ResumeFileName;
                model.ResumeFilePath = user.JobSeekerProfile.ResumeFilePath;
            }
            else if (user.Role == "Employer" && user.CompanyProfile != null)
            {
                model.CompanyName = user.CompanyProfile.CompanyName;
                model.CompanyDescription = user.CompanyProfile.Description;
                model.Website = user.CompanyProfile.Website;
                model.CompanyLocation = user.CompanyProfile.Location;
                model.Industry = user.CompanyProfile.Industry;
                model.CompanySize = user.CompanyProfile.CompanySize;
            }

            return View(model);
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            int userId = GetCurrentUserId();
            var user = await _context.Users
                .Include(u => u.CompanyProfile)
                .Include(u => u.JobSeekerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                user.FullName = model.FullName.Trim();
                user.PhoneNumber = model.PhoneNumber?.Trim();
                user.Bio = model.Bio?.Trim();

                // Handle Profile Picture upload
                if (model.ProfilePictureFile != null && model.ProfilePictureFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
                    Directory.CreateDirectory(uploadsFolder);
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ProfilePictureFile.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePictureFile.CopyToAsync(fileStream);
                    }
                    user.ProfilePicture = $"/uploads/avatars/{uniqueFileName}";
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

                    // Handle Resume Upload
                    if (model.ResumeUpload != null && model.ResumeUpload.Length > 0)
                    {
                        string resumeFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
                        Directory.CreateDirectory(resumeFolder);
                        string uniqueResumeName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ResumeUpload.FileName)}";
                        string resumeFilePath = Path.Combine(resumeFolder, uniqueResumeName);
                        using (var fileStream = new FileStream(resumeFilePath, FileMode.Create))
                        {
                            await model.ResumeUpload.CopyToAsync(fileStream);
                        }
                        user.JobSeekerProfile.ResumeFileName = model.ResumeUpload.FileName;
                        user.JobSeekerProfile.ResumeFilePath = $"/uploads/resumes/{uniqueResumeName}";
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

                // Re-sign in to update claim name if it changed
                await SignInUserAsync(user, true);

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(User user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("ProfilePicture", user.ProfilePicture ?? string.Empty)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }
    }
}
