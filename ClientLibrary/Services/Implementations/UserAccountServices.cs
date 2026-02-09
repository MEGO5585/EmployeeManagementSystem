using System.Net.Http.Json;
using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;

namespace ClientLibrary.Services.Implementations;

public class UserAccountServices(GetHttpClient getHttpClient) : IUserAccountServices
{
    public const string AuthUrl = "api/authentication";
    public async Task<GeneralResponse> CreateAsync(Register user)
    {
        var HttpClient = getHttpClient.GetPublicHttpClient();
        var result = await HttpClient.PostAsJsonAsync($"{AuthUrl}/register", user);
        if (!result.IsSuccessStatusCode) return new GeneralResponse(false, "Error occured");

        return await result.Content.ReadFromJsonAsync<GeneralResponse>() ?? new GeneralResponse(false, "Unknown error occurred");
    }
    public async Task<LoginResponse> SignInAsync(LogIn user)
    {
        var HttpClient = getHttpClient.GetPublicHttpClient();
        var result = await HttpClient.PostAsJsonAsync($"{AuthUrl}/login", user);
        if (!result.IsSuccessStatusCode) return new LoginResponse(false, "Error occured");

        return await result.Content.ReadFromJsonAsync<LoginResponse>() ?? new LoginResponse(false, "error in login");
    }

    public Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
    {
        throw new NotImplementedException();
    }
    public async Task<WeatherForecast[]> GetWeatherForecastsAsync()
    {
        var HttpClient = getHttpClient.GetPublicHttpClient();
        var result = await HttpClient.GetFromJsonAsync<WeatherForecast[]>("api/weatherforecast");
        return result!;
    }


}
