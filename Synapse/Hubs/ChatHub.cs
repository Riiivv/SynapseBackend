using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Synapse.Backend.Data;
using System.Security.Claims;

namespace Synapse.Backend.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SynapseDbContext _context;

    public ChatHub(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task JoinChannel(int channelId)
    {
        var userId = GetCurrentUserId();

        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null)
        {
            throw new HubException("Channel findes ikke.");
        }

        var isMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == channel.ServerId &&
                sm.UserId == userId);

        if (!isMember)
        {
            throw new HubException("Du har ikke adgang til denne channel.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"channel-{channelId}"
        );
    }

    public async Task LeaveChannel(int channelId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"channel-{channelId}"
        );
    }

    public async Task JoinPrivateChat(int privateChatId)
    {
        var userId = GetCurrentUserId();

        var chatExists = await _context.PrivateChats
            .AnyAsync(pc => pc.Id == privateChatId);

        if (!chatExists)
        {
            throw new HubException("Private chat findes ikke.");
        }

        var isMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == userId);

        if (!isMember)
        {
            throw new HubException("Du har ikke adgang til denne private chat.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"private-{privateChatId}"
        );
    }

    public async Task LeavePrivateChat(int privateChatId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"private-{privateChatId}"
        );
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?
            .FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            throw new HubException("Brugeren er ikke authenticated.");
        }

        return int.Parse(userIdClaim.Value);
    }
}