using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using System.Security.Cryptography;
using System.Text;

namespace Synapse.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<UserResponse>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();

        return users
            .Select(MapUser)
            .ToList();
    }

    public async Task<ServiceResult<UserResponse>> GetByIdAsync(
        int id)
    {
        var user = await _userRepository.GetByIdAsync(id);

        if (user == null)
        {
            return ServiceResult<UserResponse>
                .NotFound("Bruger findes ikke.");
        }

        return ServiceResult<UserResponse>
            .Success(MapUser(user));
    }

    public async Task<ServiceResult<bool>> UpdatePublicKeyAsync(
        int currentUserId,
        UpdatePublicKeyRequest request)
    {
        var user = await _userRepository
            .GetByIdAsync(currentUserId);

        if (user == null)
        {
            return ServiceResult<bool>
                .NotFound("Bruger findes ikke.");
        }

        var publicKey = request.PublicKey.Trim();

        if (string.IsNullOrWhiteSpace(publicKey))
        {
            return ServiceResult<bool>
                .BadRequest("Public key må ikke være tom.");
        }

        var keyBytes =
            Encoding.UTF8.GetBytes(publicKey);

        var hash =
            SHA256.HashData(keyBytes);

        var fingerprint =
            Convert.ToHexString(hash);

        // Samme key er allerede registreret.
        if (user.PublicKeyFingerprint == fingerprint)
        {
            return ServiceResult<bool>.Success(true);
        }

        user.PublicKey = publicKey;
        user.PublicKeyFingerprint = fingerprint;
        user.PublicKeyVersion += 1;
        user.PublicKeyUpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<UserPublicKeyResponse>> GetPublicKeyAsync(
        int userId)
    {
        var user = await _userRepository
            .GetByIdAsync(userId);

        if (user == null)
        {
            return ServiceResult<UserPublicKeyResponse>
                .NotFound("Bruger findes ikke.");
        }

        if (string.IsNullOrWhiteSpace(user.PublicKey))
        {
            return ServiceResult<UserPublicKeyResponse>
                .NotFound(
                    "Brugeren har ikke registreret en public key.");
        }

        var response = new UserPublicKeyResponse
        {
            Id = user.Id,
            Username = user.Username,

            PublicKey = user.PublicKey,

            PublicKeyFingerprint =
                user.PublicKeyFingerprint ?? string.Empty,

            PublicKeyVersion =
                user.PublicKeyVersion,

            PublicKeyUpdatedAt =
                user.PublicKeyUpdatedAt
        };

        return ServiceResult<UserPublicKeyResponse>
            .Success(response);
    }

    private static UserResponse MapUser(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email
        };
    }
}