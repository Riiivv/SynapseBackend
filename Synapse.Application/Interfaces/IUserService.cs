using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IUserService
{
    Task<List<UserResponse>> GetAllAsync();

    Task<ServiceResult<UserResponse>> GetByIdAsync(int id);

    Task<ServiceResult<bool>> UpdatePublicKeyAsync(int currentUserId,UpdatePublicKeyRequest request);

    Task<ServiceResult<UserPublicKeyResponse>> GetPublicKeyAsync(int userId);
}