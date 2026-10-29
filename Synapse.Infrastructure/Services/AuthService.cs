using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = new PasswordHasher<User>();
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(
        RegisterRequest request)
    {
        var usernameExists =
            await _userRepository.UsernameExistsAsync(
                request.Username);

        if (usernameExists)
        {
            return ServiceResult<AuthResponse>.BadRequest(
                "Brugernavnet findes allerede.");
        }

        var emailExists =
            await _userRepository.EmailExistsAsync(
                request.Email);

        if (emailExists)
        {
            return ServiceResult<AuthResponse>.BadRequest(
                "Email findes allerede.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                request.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var token = CreateToken(user);

        var response = new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token
        };

        return ServiceResult<AuthResponse>
            .Success(response);
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(
        LoginRequest request)
    {
        var user =
            await _userRepository.GetByUsernameAsync(
                request.Username);

        if (user == null)
        {
            return ServiceResult<AuthResponse>.Unauthorized(
                "Forkert brugernavn eller password.");
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                request.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return ServiceResult<AuthResponse>.Unauthorized(
                "Forkert brugernavn eller password.");
        }

        var token = CreateToken(user);

        var response = new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token
        };

        return ServiceResult<AuthResponse>
            .Success(response);
    }

    private string CreateToken(User user)
    {
        var jwt =
            _configuration.GetSection("Jwt");

        var keyValue = jwt["Key"];

        if (string.IsNullOrWhiteSpace(keyValue))
        {
            throw new InvalidOperationException(
                "Jwt:Key mangler i configuration.");
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()),

            new Claim(
                ClaimTypes.Name,
                user.Username)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(keyValue));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer: jwt["Issuer"],
                audience: jwt["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}