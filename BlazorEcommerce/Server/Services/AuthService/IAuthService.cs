namespace BlazorEcommerce.Server.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> RegisterUser(User user, string password);
        Task<bool> UserExists(string email);
        Task<ServiceResponse<string>> Login(string email, string password);
        Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword);
        int GetCurrentUserId();
        string GetUserEmail();
        Task<User> GetUserByEmail(string email);
    }
}
