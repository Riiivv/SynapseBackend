using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synapse.Backend.Data;
using Synapse.Backend.DTOs;
using Synapse.Backend.Models;
using System.Security.Claims;

namespace Synapse.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChannelsController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public ChannelsController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<ChannelResponse>>> GetServerChannels(
        int serverId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var serverExists = await _context.Servers
            .AnyAsync(s => s.Id == serverId);

        if (!serverExists)
        {
            return NotFound("Server findes ikke.");
        }

        var isMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var channels = await _context.Channels
            .Where(c => c.ServerId == serverId)
            .Select(c => new ChannelResponse
            {
                Id = c.Id,
                Name = c.Name,
                ServerId = c.ServerId,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(channels);
    }

    [HttpPost]
    public async Task<ActionResult<ChannelResponse>> CreateChannel(
        CreateChannelRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == request.ServerId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == request.ServerId &&
                sm.UserId == userId);

        if (membership == null)
        {
            return Forbid();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return Forbid();
        }

        var channel = new Channel
        {
            Name = request.Name,
            ServerId = request.ServerId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Channels.Add(channel);

        await _context.SaveChangesAsync();

        var response = new ChannelResponse
        {
            Id = channel.Id,
            Name = channel.Name,
            ServerId = channel.ServerId,
            CreatedAt = channel.CreatedAt
        };

        return Ok(response);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChannelResponse>> UpdateChannel(
        int id,
        UpdateChannelRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id);

        if (channel == null)
        {
            return NotFound("Channel findes ikke.");
        }

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == channel.ServerId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == server.Id &&
                sm.UserId == userId);

        if (membership == null)
        {
            return Forbid();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return Forbid();
        }

        channel.Name = request.Name;

        await _context.SaveChangesAsync();

        var response = new ChannelResponse
        {
            Id = channel.Id,
            Name = channel.Name,
            ServerId = channel.ServerId,
            CreatedAt = channel.CreatedAt
        };

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChannel(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id);

        if (channel == null)
        {
            return NotFound("Channel findes ikke.");
        }

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == channel.ServerId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == server.Id &&
                sm.UserId == userId);

        if (membership == null)
        {
            return Forbid();
        }

        if (membership.Role != "Owner" &&
            membership.Role != "Admin")
        {
            return Forbid();
        }

        var messages = await _context.Messages
            .Where(m => m.ChannelId == id)
            .ToListAsync();

        _context.Messages.RemoveRange(messages);
        _context.Channels.Remove(channel);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}