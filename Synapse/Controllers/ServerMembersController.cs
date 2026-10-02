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
public class ServerMembersController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public ServerMembersController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<ServerMemberResponse>>> GetServerMembers(
        int serverId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == serverId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var isMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == currentUserId);

        if (!isMember)
        {
            return Forbid();
        }

        var members = await _context.ServerMembers
            .Where(sm => sm.ServerId == serverId)
            .Select(sm => new ServerMemberResponse
            {
                Id = sm.Id,
                ServerId = sm.ServerId,
                UserId = sm.UserId,
                Role = sm.Role,
                JoinedAt = sm.JoinedAt,

                User = sm.User == null
                    ? null
                    : new UserResponse
                    {
                        Id = sm.User.Id,
                        Username = sm.User.Username,
                        Email = sm.User.Email
                    }
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpPost]
    public async Task<ActionResult<ServerMemberResponse>> AddMember(
        AddServerMemberRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == request.ServerId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var currentMembership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == request.ServerId &&
                sm.UserId == currentUserId);

        if (currentMembership == null)
        {
            return Forbid();
        }

        if (currentMembership.Role != "Owner" &&
            currentMembership.Role != "Admin")
        {
            return Forbid();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
        {
            return NotFound("Bruger findes ikke.");
        }

        var alreadyMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == request.ServerId &&
                sm.UserId == request.UserId);

        if (alreadyMember)
        {
            return BadRequest("Brugeren er allerede medlem af serveren.");
        }

        var serverMember = new ServerMember
        {
            ServerId = request.ServerId,
            UserId = request.UserId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        _context.ServerMembers.Add(serverMember);

        await _context.SaveChangesAsync();

        var response = new ServerMemberResponse
        {
            Id = serverMember.Id,
            ServerId = serverMember.ServerId,
            UserId = serverMember.UserId,
            Role = serverMember.Role,
            JoinedAt = serverMember.JoinedAt,

            User = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        };

        return Ok(response);
    }

    [HttpPut("server/{serverId}/user/{userId}/role")]
    public async Task<ActionResult<ServerMemberResponse>> UpdateRole(
        int serverId,
        int userId,
        UpdateServerMemberRoleRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == serverId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var currentMembership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == currentUserId);

        if (currentMembership == null ||
            currentMembership.Role != "Owner")
        {
            return Forbid();
        }

        var membership = await _context.ServerMembers
            .Include(sm => sm.User)
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == userId);

        if (membership == null)
        {
            return NotFound("Brugeren er ikke medlem af serveren.");
        }

        if (membership.Role == "Owner")
        {
            return BadRequest("Owner-rollen kan ikke ændres.");
        }

        var role = request.Role.Trim();

        if (role != "Admin" && role != "Member")
        {
            return BadRequest("Role skal være enten Admin eller Member.");
        }

        membership.Role = role;

        await _context.SaveChangesAsync();

        var response = new ServerMemberResponse
        {
            Id = membership.Id,
            ServerId = membership.ServerId,
            UserId = membership.UserId,
            Role = membership.Role,
            JoinedAt = membership.JoinedAt,

            User = membership.User == null
                ? null
                : new UserResponse
                {
                    Id = membership.User.Id,
                    Username = membership.User.Username,
                    Email = membership.User.Email
                }
        };

        return Ok(response);
    }

    [HttpDelete("server/{serverId}/user/{userId}")]
    public async Task<IActionResult> RemoveMember(
        int serverId,
        int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var server = await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == serverId);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        var currentMembership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == currentUserId);

        if (currentMembership == null)
        {
            return Forbid();
        }

        if (currentMembership.Role != "Owner" &&
            currentMembership.Role != "Admin")
        {
            return Forbid();
        }

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == userId);

        if (membership == null)
        {
            return NotFound("Brugeren er ikke medlem af serveren.");
        }

        if (membership.Role == "Owner")
        {
            return BadRequest("Server-ejeren kan ikke fjernes.");
        }

        if (currentMembership.Role == "Admin" &&
            membership.Role == "Admin")
        {
            return Forbid();
        }

        _context.ServerMembers.Remove(membership);

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

        var currentUserId = int.Parse(userIdClaim.Value);

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == currentUserId);

        if (membership == null)
        {
            return BadRequest("Du er ikke medlem af serveren.");
        }

        if (membership.Role == "Owner")
        {
            return BadRequest(
                "Server-ejeren kan ikke forlade sin egen server."
            );
        }

        _context.ServerMembers.Remove(membership);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}