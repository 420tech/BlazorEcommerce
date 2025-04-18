﻿namespace BlazorEcommerce.Server.Services.AuthService
{
    public interface IAuthService
    {
        Task<ServiceResponse<int>> RegisterUser(User user, string password);
        Task<bool> UserExists(string email);
    }
}
