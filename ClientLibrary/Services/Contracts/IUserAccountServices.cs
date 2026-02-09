using System;
using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ClientLibrary.Services.Contracts;

public interface IUserAccountServices
{
    Task<GeneralResponse> CreateAsync(Register user);
    Task<LoginResponse> SignInAsync(LogIn user);
    Task<LoginResponse> RefreshTokenAsync(RefreshToken token);
    Task<WeatherForecast[]> GetWeatherForecastsAsync();

}
