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
using JobPortPro.Models;
using JobPortPro.Services;

namespace JobPortPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(IAuthService authService, IWebHostEnvironment webHostEnvironment)
        {
            _authService = authService;
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
                var (success, errorMessage, user) = await _authService.RegisterAsync(model);
                if (!success)
                {
                    ModelState.AddModelError("Email", errorMessage);
                    return View(model);
                }

                if (user != null)
                {
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
                var user = await _authService.ValidateCredentialsAsync(model.Email, model.Password);

                if (user != null)
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

        // GET & POST: /Account/Logout
        [AcceptVerbs("GET", "POST")]
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
            var user = await _authService.GetUserByIdAsync(userId);

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
            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Sync Immutable Fields from Verified Account
            model.Email = user.Email;
            model.Role = user.Role;
            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.Role));

            if (ModelState.IsValid)
            {
                string? avatarPath = null;
                string? resumePath = null;
                string? resumeFileName = null;

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
                    avatarPath = $"/uploads/avatars/{uniqueFileName}";
                }

                // Handle Resume upload for JobSeeker
                if (user.Role == "JobSeeker" && model.ResumeUpload != null && model.ResumeUpload.Length > 0)
                {
                    string resumeFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "resumes");
                    Directory.CreateDirectory(resumeFolder);
                    string uniqueResumeName = $"{Guid.NewGuid()}_{Path.GetFileName(model.ResumeUpload.FileName)}";
                    string resumeFilePath = Path.Combine(resumeFolder, uniqueResumeName);
                    using (var fileStream = new FileStream(resumeFilePath, FileMode.Create))
                    {
                        await model.ResumeUpload.CopyToAsync(fileStream);
                    }
                    resumeFileName = model.ResumeUpload.FileName;
                    resumePath = $"/uploads/resumes/{uniqueResumeName}";
                }

                await _authService.UpdateProfileAsync(userId, model, avatarPath, resumePath, resumeFileName);

                user = await _authService.GetUserByIdAsync(userId);
                if (user != null)
                {
                    await SignInUserAsync(user, true);
                }

                TempData["SuccessMessage"] = "Your profile has been updated successfully!";
                return RedirectToAction(nameof(Profile));
            }

            // Repopulate Existing Display Values on Validation Failure
            model.ProfilePicture = user.ProfilePicture;
            if (user.Role == "JobSeeker" && user.JobSeekerProfile != null)
            {
                model.ResumeFileName = user.JobSeekerProfile.ResumeFileName;
                model.ResumeFilePath = user.JobSeekerProfile.ResumeFilePath;
            }

            return View(model);
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _authService.CheckEmailExistsAsync(model.Email);
                if (exists)
                {
                    return RedirectToAction(nameof(ResetPassword), new { email = model.Email });
                }

                ModelState.AddModelError("Email", "No account registered with this email address.");
            }
            return View(model);
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction(nameof(ForgotPassword));
            }
            return View(new ResetPasswordViewModel { Email = email });
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool updated = await _authService.ResetPasswordAsync(model.Email, model.NewPassword);
                if (updated)
                {
                    TempData["SuccessMessage"] = "Your password has been reset successfully. Please sign in with your new password.";
                    return RedirectToAction(nameof(Login));
                }

                ModelState.AddModelError(string.Empty, "Unable to reset password. Please check your email address.");
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
