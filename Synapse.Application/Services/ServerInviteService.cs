using Synapse.Application.Common;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class ServerInviteService : IServerInviteService
{
    private readonly IServerRepository _serverRepository;
    private readonly IServerMemberRepository _serverMemberRepository;
    private readonly IServerInviteRepository _serverInviteRepository;

    public ServerInviteService(
        IServerRepository serverRepository,
        IServerMemberRepository serverMemberRepository,
        IServerInviteRepository serverInviteRepository)
    {
        _serverRepository = serverRepository;
        _serverMemberRepository = serverMemberRepository;
        _serverInviteRepository = serverInviteRepository;
    }

    public async Task<ServiceResult<object>> CreateInviteAsync(
        int currentUserId,
        int serverId)
    {
        var server = await _serverRepository.GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<object>
                .NotFound("Server findes ikke.");
        }

        var membership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                currentUserId);

        if (membership == null)
        {
            return ServiceResult<object>.Forbidden();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return ServiceResult<object>.Forbidden();
        }

        var invite = new ServerInvite
        {
            Code = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper(),

            ServerId = serverId,
            CreatedByUserId = currentUserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        await _serverInviteRepository.AddAsync(invite);
        await _serverInviteRepository.SaveChangesAsync();

        var response = new
        {
            invite.Id,
            invite.Code,
            invite.ServerId,
            invite.CreatedAt,
            invite.ExpiresAt
        };

        return ServiceResult<object>.Success(response);
    }

    public async Task<ServiceResult<object>> JoinServerAsync(
        int currentUserId,
        string code)
    {
        code = code.Trim().ToUpper();

        var invite =
            await _serverInviteRepository.GetByCodeAsync(code);

        if (invite == null)
        {
            return ServiceResult<object>
                .NotFound("Invite findes ikke.");
        }

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            return ServiceResult<object>
                .BadRequest("Invite er udløbet.");
        }

        var alreadyMember =
            await _serverMemberRepository.IsMemberAsync(
                invite.ServerId,
                currentUserId);

        if (alreadyMember)
        {
            return ServiceResult<object>
                .BadRequest(
                    "Du er allerede medlem af serveren.");
        }

        var membership = new ServerMember
        {
            ServerId = invite.ServerId,
            UserId = currentUserId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        await _serverMemberRepository.AddAsync(membership);
        await _serverMemberRepository.SaveChangesAsync();

        var response = new
        {
            message = "Du er nu medlem af serveren.",
            serverId = invite.ServerId
        };

        return ServiceResult<object>.Success(response);
    }

    public async Task<ServiceResult<bool>> DeleteInviteAsync(
        int currentUserId,
        int inviteId)
    {
        var invite =
            await _serverInviteRepository.GetByIdAsync(inviteId);

        if (invite == null)
        {
            return ServiceResult<bool>
                .NotFound("Invite findes ikke.");
        }

        var membership =
            await _serverMemberRepository.GetMembershipAsync(
                invite.ServerId,
                currentUserId);

        if (membership == null)
        {
            return ServiceResult<bool>.Forbidden();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return ServiceResult<bool>.Forbidden();
        }

        await _serverInviteRepository.DeleteAsync(invite);
        await _serverInviteRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }
}