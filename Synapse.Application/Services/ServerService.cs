using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class ServerService : IServerService
{
    private readonly IServerRepository _serverRepository;
    private readonly IUserRepository _userRepository;
    private readonly IServerMemberRepository _serverMemberRepository;
    private readonly IServerInviteRepository _serverInviteRepository;
    private readonly IChannelRepository _channelRepository;
    private readonly IMessageRepository _messageRepository;

    public ServerService(
        IServerRepository serverRepository,
        IUserRepository userRepository,
        IServerMemberRepository serverMemberRepository,
        IServerInviteRepository serverInviteRepository,
        IChannelRepository channelRepository,
        IMessageRepository messageRepository)
    {
        _serverRepository = serverRepository;
        _userRepository = userRepository;
        _serverMemberRepository = serverMemberRepository;
        _serverInviteRepository = serverInviteRepository;
        _channelRepository = channelRepository;
        _messageRepository = messageRepository;
    }

    public async Task<List<ServerResponse>> GetAllAsync()
    {
        var servers = await _serverRepository.GetAllAsync();

        var responses = new List<ServerResponse>();

        foreach (var server in servers)
        {
            responses.Add(await MapServerAsync(server));
        }

        return responses;
    }

    public async Task<ServerResponse?> GetByIdAsync(int id)
    {
        var server = await _serverRepository.GetByIdAsync(id);

        if (server == null)
        {
            return null;
        }

        return await MapServerAsync(server);
    }

    public async Task<List<ServerResponse>> GetForUserAsync(
        int userId)
    {
        var servers =
            await _serverRepository.GetServersForUserAsync(userId);

        var responses = new List<ServerResponse>();

        foreach (var server in servers)
        {
            responses.Add(await MapServerAsync(server));
        }

        return responses;
    }

    public async Task<ServerResponse> CreateAsync(
        int userId,
        CreateServerRequest request)
    {
        var server = new Server
        {
            Name = request.Name,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _serverRepository.AddAsync(server);

        // Første SaveChanges giver serveren dens database-ID.
        await _serverRepository.SaveChangesAsync();

        var serverMember = new ServerMember
        {
            ServerId = server.Id,
            UserId = userId,
            Role = "Owner",
            JoinedAt = DateTime.UtcNow
        };

        await _serverMemberRepository.AddAsync(serverMember);

        await _serverMemberRepository.SaveChangesAsync();

        return await MapServerAsync(server);
    }

    public async Task<ServiceResult<ServerResponse>> UpdateAsync(
        int serverId,
        int userId,
        UpdateServerRequest request)
    {
        var server =
            await _serverRepository.GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<ServerResponse>
                .NotFound("Server findes ikke.");
        }

        if (server.OwnerId != userId)
        {
            return ServiceResult<ServerResponse>.Forbidden();
        }

        server.Name = request.Name;

        await _serverRepository.UpdateAsync(server);
        await _serverRepository.SaveChangesAsync();

        var response = await MapServerAsync(server);

        return ServiceResult<ServerResponse>.Success(response);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        int serverId,
        int userId)
    {
        var server =
            await _serverRepository.GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<bool>
                .NotFound("Server findes ikke.");
        }

        if (server.OwnerId != userId)
        {
            return ServiceResult<bool>.Forbidden();
        }

        // Slet invites
        var invites =
            await _serverInviteRepository
                .GetByServerIdAsync(serverId);

        foreach (var invite in invites)
        {
            await _serverInviteRepository.DeleteAsync(invite);
        }

        // Slet channels + deres messages
        var channels =
            await _channelRepository
                .GetByServerIdAsync(serverId);

        foreach (var channel in channels)
        {
            var messages =
                await _messageRepository
                    .GetChannelMessagesAsync(channel.Id);

            foreach (var message in messages)
            {
                await _messageRepository.DeleteAsync(message);
            }

            await _channelRepository.DeleteAsync(channel);
        }

        // Slet server members
        var members =
            await _serverMemberRepository
                .GetByServerIdAsync(serverId);

        foreach (var member in members)
        {
            await _serverMemberRepository.DeleteAsync(member);
        }

        // Til sidst selve serveren
        await _serverRepository.DeleteAsync(server);

        await _serverRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> LeaveAsync(
        int serverId,
        int userId)
    {
        var server =
            await _serverRepository.GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<bool>
                .NotFound("Server findes ikke.");
        }

        if (server.OwnerId == userId)
        {
            return ServiceResult<bool>.BadRequest(
                "Server-ejeren kan ikke forlade sin egen server.");
        }

        var membership =
            await _serverMemberRepository.GetMembershipAsync(
                serverId,
                userId);

        if (membership == null)
        {
            return ServiceResult<bool>.BadRequest(
                "Du er ikke medlem af serveren.");
        }

        await _serverMemberRepository.DeleteAsync(membership);

        await _serverMemberRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private async Task<ServerResponse> MapServerAsync(
        Server server)
    {
        var owner =
            await _userRepository.GetByIdAsync(server.OwnerId);

        return new ServerResponse
        {
            Id = server.Id,
            Name = server.Name,
            OwnerId = server.OwnerId,
            CreatedAt = server.CreatedAt,

            Owner = owner == null
                ? null
                : new UserResponse
                {
                    Id = owner.Id,
                    Username = owner.Username,
                    Email = owner.Email
                }
        };
    }
}