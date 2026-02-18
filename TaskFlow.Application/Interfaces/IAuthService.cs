namespace TaskFlow.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> ValidateUserAsync(string userName, string password);
        Task<string?> GenerateTokenAsync(string userName);
        Task<bool> IsInRoleAsync(string userName, string role);
    }
}
