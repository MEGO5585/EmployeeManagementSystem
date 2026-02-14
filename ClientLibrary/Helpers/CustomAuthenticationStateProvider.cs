using System;
using System.Security.Claims;
using BaseLibrary.DTOs;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Authorization;

namespace ClientLibrary.Helpers;

public class CustomAuthenticationStateProvider(LocalStorageServices localStorageServices) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal anonymous = new(new ClaimsIdentity());
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var stringToken = await localStorageServices.GetToken();
        if (string.IsNullOrEmpty(stringToken)) return await Task.FromResult(new AuthenticationState(anonymous));

        var deserializeToken = Serializations.DeserializeJsonString<UserSession>(stringToken);
        if (deserializeToken == null) return await Task.FromResult(new AuthenticationState(anonymous));

        var getUserClaims = DecryptToken(deserializeToken.Token);
        if (getUserClaims == null) return await Task.FromResult(new AuthenticationState(anonymous));

        var claimsPrincipal = SetClaimsPrincipal(getUserClaims);
        return await Task.FromResult(new AuthenticationState(claimsPrincipal));

    }

    public async Task UpdateAuthenticationState(UserSession userSession)
    {
        var claimsPrincipal = new ClaimsPrincipal();
        if(userSession.Token != null || userSession.RefreshToken != null)
        {
            var SerializeSession = Serializations.Serializeobj(userSession);
            await localStorageServices.SetToken(SerializeSession);
            var getUserClaims = DecryptToken(userSession.Token);
            claimsPrincipal = SetClaimsPrincipal(getUserClaims);
        }


    }
    
    private static CustomUserClaims DecryptToken(string? jwtToken)
    {
        if (string.IsNullOrEmpty(jwtToken)) return new CustomUserClaims();

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwtToken);

        var userId = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
        var Name = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
        var Email = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email);
        var Role = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role);
        return new CustomUserClaims(userId!.Value, Name!.Value, Email!.Value, Role!.Value);
    }

    public static ClaimsPrincipal SetClaimsPrincipal(CustomUserClaims claims)
    {
        if (claims.Email is null) return new ClaimsPrincipal();
        return new ClaimsPrincipal(new ClaimsIdentity(
            new List<Claim>
            {
                new (ClaimTypes.NameIdentifier,claims.Id),
                new (ClaimTypes.Name,claims.Name),
                new (ClaimTypes.Email,claims.Email),
                new (ClaimTypes.Role,claims.Role),
            },
            "JwtAuth"
        ));
    }

}
