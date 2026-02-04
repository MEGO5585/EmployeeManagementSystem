using System;
using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ServerLibrary.Repostories.Contracts;

public interface IUserAccount
{
    Task<GeneralResponse> CreateAsync(Register user);
    Task<LoginResponse> SignInAsync(LogIn user);
    Task<LoginResponse> RefreshTokenAsync(RefreshToken token);

}
