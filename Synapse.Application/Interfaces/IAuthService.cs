using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(
        RegisterRequest request);

    Task<ServiceResult<AuthResponse>> LoginAsync(
        LoginRequest request);
}