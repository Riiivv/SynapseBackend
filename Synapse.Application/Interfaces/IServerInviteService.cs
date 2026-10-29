using Synapse.Application.Common;

namespace Synapse.Application.Interfaces;

public interface IServerInviteService
{
    Task<ServiceResult<object>> CreateInviteAsync(int currentUserId,int serverId);

    Task<ServiceResult<object>> JoinServerAsync(int currentUserId,string code);

    Task<ServiceResult<bool>> DeleteInviteAsync(int currentUserId,int inviteId);
}