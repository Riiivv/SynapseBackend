using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;

namespace Synapse.Application.Services;

public class ChannelService : IChannelService
{
    private readonly IChannelRepository _channelRepository;
    private readonly IServerRepository _serverRepository;
    private readonly IServerMemberRepository _serverMemberRepository;
    private readonly IMessageRepository _messageRepository;

    public ChannelService(
        IChannelRepository channelRepository,
        IServerRepository serverRepository,
        IServerMemberRepository serverMemberRepository,
        IMessageRepository messageRepository)
    {
        _channelRepository = channelRepository;
        _serverRepository = serverRepository;
        _serverMemberRepository = serverMemberRepository;
        _messageRepository = messageRepository;
    }

    public async Task<ServiceResult<List<ChannelResponse>>> GetServerChannelsAsync(
        int serverId,
        int userId)
    {
        var server = await _serverRepository.GetByIdAsync(serverId);

        if (server == null)
        {
            return ServiceResult<List<ChannelResponse>>
                .NotFound("Server findes ikke.");
        }

        var isMember = await _serverMemberRepository
            .IsMemberAsync(serverId, userId);

        if (!isMember)
        {
            return ServiceResult<List<ChannelResponse>>
                .Forbidden();
        }

        var channels = await _channelRepository
            .GetByServerIdAsync(serverId);

        var response = channels
            .Select(MapChannel)
            .ToList();

        return ServiceResult<List<ChannelResponse>>
            .Success(response);
    }

    public async Task<ServiceResult<ChannelResponse>> CreateAsync(
        int userId,
        CreateChannelRequest request)
    {
        var server = await _serverRepository
            .GetByIdAsync(request.ServerId);

        if (server == null)
        {
            return ServiceResult<ChannelResponse>
                .NotFound("Server findes ikke.");
        }

        var membership = await _serverMemberRepository
            .GetMembershipAsync(request.ServerId, userId);

        if (membership == null)
        {
            return ServiceResult<ChannelResponse>
                .Forbidden();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return ServiceResult<ChannelResponse>
                .Forbidden();
        }

        var channel = new Channel
        {
            Name = request.Name,
            ServerId = request.ServerId,
            CreatedAt = DateTime.UtcNow
        };

        await _channelRepository.AddAsync(channel);
        await _channelRepository.SaveChangesAsync();

        return ServiceResult<ChannelResponse>
            .Success(MapChannel(channel));
    }

    public async Task<ServiceResult<ChannelResponse>> UpdateAsync(
        int channelId,
        int userId,
        UpdateChannelRequest request)
    {
        var channel = await _channelRepository
            .GetByIdAsync(channelId);

        if (channel == null)
        {
            return ServiceResult<ChannelResponse>
                .NotFound("Channel findes ikke.");
        }

        var server = await _serverRepository
            .GetByIdAsync(channel.ServerId);

        if (server == null)
        {
            return ServiceResult<ChannelResponse>
                .NotFound("Server findes ikke.");
        }

        var membership = await _serverMemberRepository
            .GetMembershipAsync(server.Id, userId);

        if (membership == null)
        {
            return ServiceResult<ChannelResponse>
                .Forbidden();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return ServiceResult<ChannelResponse>
                .Forbidden();
        }

        channel.Name = request.Name;

        await _channelRepository.UpdateAsync(channel);
        await _channelRepository.SaveChangesAsync();

        return ServiceResult<ChannelResponse>
            .Success(MapChannel(channel));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(
        int channelId,
        int userId)
    {
        var channel = await _channelRepository
            .GetByIdAsync(channelId);

        if (channel == null)
        {
            return ServiceResult<bool>
                .NotFound("Channel findes ikke.");
        }

        var server = await _serverRepository
            .GetByIdAsync(channel.ServerId);

        if (server == null)
        {
            return ServiceResult<bool>
                .NotFound("Server findes ikke.");
        }

        var membership = await _serverMemberRepository
            .GetMembershipAsync(server.Id, userId);

        if (membership == null)
        {
            return ServiceResult<bool>
                .Forbidden();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return ServiceResult<bool>
                .Forbidden();
        }

        var messages = await _messageRepository
            .GetChannelMessagesAsync(channelId);

        foreach (var message in messages)
        {
            await _messageRepository.DeleteAsync(message);
        }

        await _channelRepository.DeleteAsync(channel);

        await _channelRepository.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    private static ChannelResponse MapChannel(Channel channel)
    {
        return new ChannelResponse
        {
            Id = channel.Id,
            Name = channel.Name,
            ServerId = channel.ServerId,
            CreatedAt = channel.CreatedAt
        };
    }
}