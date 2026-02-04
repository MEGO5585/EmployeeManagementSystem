using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BaseLibrary.DTOs;
using BaseLibrary.Entities;
using BaseLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ServerLibrary.Data;
using ServerLibrary.Helpers;
using ServerLibrary.Repostories.Contracts;

namespace ServerLibrary.Repostories.Implementations;

public class UserAccountRepository(IOptions<JwtSection> config, AppDbContext appDbContext) : IUserAccount
{
    public async Task<GeneralResponse> CreateAsync(Register user)
    {
        if (user is null) return new GeneralResponse(false, "Model is empty");
        // Check Email in the database
        var userCheck = await FindUserByEmail(user.Email!);
        if (userCheck is not null) return new GeneralResponse(false, "User already registed");
        // save user
        var applicationUser = await AddToDatabase(new ApplicationUser()
        {
            FullName = user.FullName,
            Email = user.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(user.Password)
        });
        // Check ,create and assign Role
        var checkAdminRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Name!.Equals(Constants.Admin));
        if (checkAdminRole is null)
        {
            var createAdminRole = await AddToDatabase(new SystemRole() { Name = Constants.Admin });
            await AddToDatabase(new UserRole() { RoleId = createAdminRole.Id, UserId = applicationUser.Id });
            return new GeneralResponse(true, "Account Created!");
        }
        var checkUserRole = await appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Name == Constants.User);
        if (checkUserRole is null)
        {
            var response = await AddToDatabase(new SystemRole() { Name = Constants.User });
            await AddToDatabase(new UserRole() { RoleId = response.Id, UserId = applicationUser.Id });
        }
        else
        {
            await AddToDatabase(new UserRole() { RoleId = checkUserRole.Id, UserId = applicationUser.Id });
        }
        return new GeneralResponse(true, "Account Created!");
    }

    public async Task<LoginResponse> SignInAsync(LogIn user)
    {
        if (user is null) return new LoginResponse(false, "model is empty");
        var applicationUser = await FindUserByEmail(user.Email!);
        if (applicationUser == null) return new LoginResponse(false, "Can't find email, Please register");
        if (!BCrypt.Net.BCrypt.Verify(user.Password, applicationUser.Password))
        {
            return new LoginResponse(false, "Password incorrect");
        }
        var getUserRole = await FindUserRole(applicationUser.Id);
        if (getUserRole is null) return new LoginResponse(false, "User role not found");

        var getRoleName = await FindRoleName(getUserRole.RoleId);
        if (getRoleName is null) return new LoginResponse(false, "User role not found");

        string jwtToken = GenerateToken(applicationUser, getRoleName.Name!);
        string RefreshToken = GenerateRefrashToken();

        // save refresh token to database
        var findUser = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.UserId == applicationUser.Id);
        if (findUser is not null)
        {
            findUser.Token = RefreshToken;
            await appDbContext.SaveChangesAsync();
        }else
        {
            await AddToDatabase(new RefreshTokenInfo() { Token = RefreshToken, UserId = applicationUser.Id });
        }
        return new LoginResponse(true, "Login Successfuly", jwtToken, RefreshToken);

    }

    private static string GenerateRefrashToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private string GenerateToken(ApplicationUser user, string role)
    {
        // Add validation for JWT configuration
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config.Value.Key!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var userClaims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName!),
            new Claim(ClaimTypes.Email,user.Email!),
            new Claim(ClaimTypes.Role,role)
        };
        var token = new JwtSecurityToken(
            issuer: config.Value.Issuer,
            audience: config.Value.Audience,
            claims: userClaims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    private async Task<UserRole> FindUserRole(int userId) => await appDbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == userId);

    private async Task<SystemRole> FindRoleName(int roleId) => await appDbContext.SystemRoles.FirstOrDefaultAsync(x => x.Id == roleId);
    private async Task<ApplicationUser?> FindUserByEmail(string email) =>
        await appDbContext.ApplicationUsers.FirstOrDefaultAsync(x =>x.Email == email);

    // Add rows to database
    private async Task<T> AddToDatabase<T>(T model)
    {
        var result = await appDbContext.AddAsync(model!);
        await appDbContext.SaveChangesAsync();
        return (T)result.Entity;
    }

    public async Task<LoginResponse> RefreshTokenAsync(RefreshToken token)
    {
        if (token is null) return new LoginResponse(false, "Model is empty");

        var findToken = await appDbContext.RefreshTokenInfos.FirstOrDefaultAsync(x => x.Token == token.Token);
        if (findToken is null) return new LoginResponse(false, "refresh token is requierd");
        // get user
        var user = await appDbContext.ApplicationUsers.FirstOrDefaultAsync(x => x.Id == findToken.UserId);
        if (user is null) return new LoginResponse(false, "Refresh token couldn't be found because user not found");
        var userRole = await FindUserRole(user.Id);
        var roleName = await FindRoleName(userRole.RoleId);
        string jwtToken = GenerateToken(user, roleName.Name!);
        string refreshToken = GenerateRefrashToken();

        var updateRefreshToken = await appDbContext.RefreshTokenInfos.
        FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (updateRefreshToken is null) return new LoginResponse(false, "Refresh token cann't be updated because user hasn't sign in yet");

        updateRefreshToken.Token = refreshToken;
        await appDbContext.SaveChangesAsync();
        return new LoginResponse(true, "token refreshed successfuly", jwtToken, refreshToken);
    }
}
