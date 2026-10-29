using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class ServerMemberService : IServerMemberService
{
    private readonly IServerRepository _serverRepository;
    private readonly IServerMemberRepository _serverMemberRepository;
    private readonly IUserRepository _userRepository;

    public ServerMemberService(
        IServerRepository serverRepository,
        IServerMemberRepository serverMemberRepository,
        IUserRepository userRepository)
    {
        _serverRepository = serverRepository;
        _serverMemberRepository = serverMemberRepository;
        _userRepository = userRepository;
    }

    public async Task<ServiceResult<List<ServerMemberResponse>>> GetServerMembersAsync(
        int serverId,
        int currentUserId)
    {
        var server = await _serverRepository.GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<List<ServerMemberResponse>>
                .NotFound("Server findes ikke.");
        }

        var isMember = await _serverMemberRepository
            .IsMemberAsync(serverId, currentUserId);

        if (!isMember)
        {
            return ServiceResult<List<ServerMemberResponse>>
                .Forbidden();
        }

        var members = await _serverMemberRepository
            .GetByServerIdAsync(serverId);

        var response = members
            .Select(MapMember)
            .ToList();

        return ServiceResult<List<ServerMemberResponse>>
            .Success(response);
    }

    public async Task<ServiceResult<ServerMemberResponse>> AddMemberAsync(
        int currentUserId,
        AddServerMemberRequest request)
    {
        var server = await _serverRepository
            .GetByIdAsync(request.ServerId);

        if (server == null)
        {
            return ServiceResult<ServerMemberResponse>
                .NotFound("Server findes ikke.");
        }

        var currentMembership =
            await _serverMemberRepository.GetMembershipAsync(
                request.ServerId,
                currentUserId);

        if (currentMembership == null)
        {
            return ServiceResult<ServerMemberResponse>
                .Forbidden();
        }

        if (currentMembership.Role != "Owner" &&
            currentMembership.Role != "Admin")
        {
            return ServiceResult<ServerMemberResponse>
                .Forbidden();
        }

        var user = await _userRepository
            .GetByIdAsync(request.UserId);

        if (user == null)
        {
            return ServiceResult<ServerMemberResponse>
                .NotFound("Bruger findes ikke.");
        }

        var alreadyMember = await _serverMemberRepository
            .IsMemberAsync(
                request.ServerId,
                request.UserId);

        if (alreadyMember)
        {
            return ServiceResult<ServerMemberResponse>
                .BadRequest(
                    "Brugeren er allerede medlem af serveren.");
        }

        var member = new ServerMember
        {
            ServerId = request.ServerId,
            UserId = request.UserId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        await _serverMemberRepository.AddAsync(member);
        await _serverMemberRepository.SaveChangesAsync();

        member.User = user;

        return ServiceResult<ServerMemberResponse>
            .Success(MapMember(member));
    }

    public async Task<ServiceResult<ServerMemberResponse>> UpdateRoleAsync(
        int serverId,
        int userId,
        int currentUserId,
        UpdateServerMemberRoleRequest request)
    {
        var server = await _serverRepository
            .GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<ServerMemberResponse>
                .NotFound("Server findes ikke.");
        }

        var currentMembership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                currentUserId);

        if (currentMembership == null ||
            currentMembership.Role != "Owner")
        {
            return ServiceResult<ServerMemberResponse>
                .Forbidden();
        }

        var membership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                userId);

        if (membership == null)
        {
            return ServiceResult<ServerMemberResponse>
                .NotFound(
                    "Brugeren er ikke medlem af serveren.");
        }

        if (membership.Role == "Owner")
        {
            return ServiceResult<ServerMemberResponse>
                .BadRequest(
                    "Owner-rollen kan ikke ændres.");
        }

        var role = request.Role.Trim();

        if (role != "Admin" &&
            role != "Member")
        {
            return ServiceResult<ServerMemberResponse>
                .BadRequest(
                    "Role skal være enten Admin eller Member.");
        }

        membership.Role = role;

        await _serverMemberRepository.UpdateAsync(membership);
        await _serverMemberRepository.SaveChangesAsync();

        return ServiceResult<ServerMemberResponse>
            .Success(MapMember(membership));
    }

    public async Task<ServiceResult<bool>> RemoveMemberAsync(
        int serverId,
        int userId,
        int currentUserId)
    {
        var server = await _serverRepository
            .GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<bool>
                .NotFound("Server findes ikke.");
        }

        var currentMembership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                currentUserId);

        if (currentMembership == null)
        {
            return ServiceResult<bool>.Forbidden();
        }

        if (currentMembership.Role != "Owner" &&
            currentMembership.Role != "Admin")
        {
            return ServiceResult<bool>.Forbidden();
        }

        var membership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                userId);

        if (membership == null)
        {
            return ServiceResult<bool>
                .NotFound(
                    "Brugeren er ikke medlem af serveren.");
        }

        if (membership.Role == "Owner")
        {
            return ServiceResult<bool>
                .BadRequest(
                    "Server-ejeren kan ikke fjernes.");
        }

        if (currentMembership.Role == "Admin" &&
            membership.Role == "Admin")
        {
            return ServiceResult<bool>.Forbidden();
        }

        await _serverMemberRepository.DeleteAsync(membership);
        await _serverMemberRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> LeaveServerAsync(
        int serverId,
        int currentUserId)
    {
        var membership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                currentUserId);

        if (membership == null)
        {
            return ServiceResult<bool>
                .BadRequest(
                    "Du er ikke medlem af serveren.");
        }

        if (membership.Role == "Owner")
        {
            return ServiceResult<bool>
                .BadRequest(
                    "Server-ejeren kan ikke forlade sin egen server.");
        }

        await _serverMemberRepository.DeleteAsync(membership);
        await _serverMemberRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private static ServerMemberResponse MapMember(
        ServerMember member)
    {
        return new ServerMemberResponse
        {
            Id = member.Id,
            ServerId = member.ServerId,
            UserId = member.UserId,
            Role = member.Role,
            JoinedAt = member.JoinedAt,

            User = member.User == null
                ? null
                : new UserResponse
                {
                    Id = member.User.Id,
                    Username = member.User.Username,
                    Email = member.User.Email
                }
        };
    }
}