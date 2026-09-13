using System.Threading.Tasks;
using JobPortPro.Models;

namespace JobPortPro.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model);
        Task<User?> ValidateCredentialsAsync(string email, string password);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> UpdateProfileAsync(int userId, ProfileViewModel model, string? avatarPath, string? resumePath, string? resumeFileName);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<bool> CheckEmailExistsAsync(string email);
    }
}
