using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IServerService
{
    Task<List<ServerResponse>> GetAllAsync();

    Task<ServerResponse?> GetByIdAsync(int id);

    Task<List<ServerResponse>> GetForUserAsync(int userId);

    Task<ServerResponse> CreateAsync(int userId,CreateServerRequest request);

    Task<ServiceResult<ServerResponse>> UpdateAsync(int serverId,int userId,UpdateServerRequest request);

    Task<ServiceResult<bool>> DeleteAsync(int serverId,int userId);

    Task<ServiceResult<bool>> LeaveAsync(int serverId,int userId);
}