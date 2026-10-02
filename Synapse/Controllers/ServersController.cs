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
public class ServersController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public ServersController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServerResponse>>> GetServers()
    {
        var servers = await _context.Servers
            .Select(s => new ServerResponse
            {
                Id = s.Id,
                Name = s.Name,
                OwnerId = s.OwnerId,

                Owner = s.Owner == null
                    ? null
                    : new UserResponse
                    {
                        Id = s.Owner.Id,
                        Username = s.Owner.Username,
                        Email = s.Owner.Email
                    },

                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(servers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerResponse>> GetServer(int id)
    {
        var server = await _context.Servers
            .Where(s => s.Id == id)
            .Select(s => new ServerResponse
            {
                Id = s.Id,
                Name = s.Name,
                OwnerId = s.OwnerId,

                Owner = s.Owner == null
                    ? null
                    : new UserResponse
                    {
                        Id = s.Owner.Id,
                        Username = s.Owner.Username,
                        Email = s.Owner.Email
                    },

                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        return Ok(server);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<ServerResponse>>> GetMyServers()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var servers = await _context.ServerMembers
            .Where(sm => sm.UserId == userId)
            .Select(sm => sm.Server)
            .Where(s => s != null)
            .Select(s => new ServerResponse
            {
                Id = s!.Id,
                Name = s.Name,
                OwnerId = s.OwnerId,

                Owner = s.Owner == null
                    ? null
                    : new UserResponse
                    {
                        Id = s.Owner.Id,
                        Username = s.Owner.Username,
                        Email = s.Owner.Email
                    },

                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(servers);
    }
    [HttpPost]
    public async Task<ActionResult<ServerResponse>> CreateServer(
        CreateServerRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var server = new Server
        {
            Name = request.Name,
            OwnerId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Servers.Add(server);

        await _context.SaveChangesAsync();

        var serverMember = new ServerMember
        {
            ServerId = server.Id,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };

        _context.ServerMembers.Add(serverMember);

        await _context.SaveChangesAsync();

        var response = new ServerResponse
        {
            Id = server.Id,
            Name = server.Name,
            OwnerId = server.OwnerId,
            CreatedAt = server.CreatedAt,

            Owner = null
        };

        return CreatedAtAction(
            nameof(GetServer),
            new { id = server.Id },
            response
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServerResponse>> UpdateServer(
        int id,
        UpdateServerRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        if (server.OwnerId != userId)
        {
            return Forbid();
        }

        server.Name = request.Name;

        await _context.SaveChangesAsync();

        var owner = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == server.OwnerId);

        var response = new ServerResponse
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

        return Ok(response);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServer(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == id);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        if (server.OwnerId != userId)
        {
            return Forbid();
        }

        var channelIds = await _context.Channels
            .Where(c => c.ServerId == id)
            .Select(c => c.Id)
            .ToListAsync();

        var messages = await _context.Messages
            .Where(m =>
                m.ChannelId != null &&
                channelIds.Contains(m.ChannelId.Value))
            .ToListAsync();

        var channels = await _context.Channels
            .Where(c => c.ServerId == id)
            .ToListAsync();

        var members = await _context.ServerMembers
            .Where(sm => sm.ServerId == id)
            .ToListAsync();

        _context.Messages.RemoveRange(messages);
        _context.Channels.RemoveRange(channels);
        _context.ServerMembers.RemoveRange(members);
        _context.Servers.Remove(server);

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("server/{serverId}/leave")]
    public async Task<IActionResult> LeaveServer(int serverId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == serverId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        if (server.OwnerId == userId)
        {
            return BadRequest(
                "Server-ejeren kan ikke forlade sin egen server."
            );
        }

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == userId);

        if (membership == null)
        {
            return BadRequest("Du er ikke medlem af serveren.");
        }

        _context.ServerMembers.Remove(membership);

        await _context.SaveChangesAsync();

        return NoContent();
    }

}