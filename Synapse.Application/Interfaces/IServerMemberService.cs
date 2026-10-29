using Synapse.Application.Common;
using Synapse.Application.DTOs;

namespace Synapse.Application.Interfaces;

public interface IServerMemberService
{
    Task<ServiceResult<List<ServerMemberResponse>>> GetServerMembersAsync(int serverId,int currentUserId);

    Task<ServiceResult<ServerMemberResponse>> AddMemberAsync(int currentUserId,AddServerMemberRequest request);

    Task<ServiceResult<ServerMemberResponse>> UpdateRoleAsync(int serverId,int userId,int currentUserId,UpdateServerMemberRoleRequest request);

    Task<ServiceResult<bool>> RemoveMemberAsync(int serverId,int userId,int currentUserId);

    Task<ServiceResult<bool>> LeaveServerAsync(int serverId,int currentUserId);
}