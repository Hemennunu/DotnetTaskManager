using TaskFlow.Domain.Entities;

namespace TaskFlow.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> GetUserByUserNameAsync(string userName);
        Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password);
        Task<ApplicationUser> UpdateUserAsync(ApplicationUser user);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> UserExistsAsync(string id);
        Task<bool> UserExistsByEmailAsync(string email);
        Task<bool> UserExistsByUserNameAsync(string userName);
    }
}
