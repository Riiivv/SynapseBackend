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
public class ServerInvitesController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public ServerInvitesController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvite(
        CreateServerInviteRequest request)
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

        var invite = new ServerInvite
        {
            Code = Guid.NewGuid()
                .ToString("N")
                .Substring(0, 8)
                .ToUpper(),

            ServerId = request.ServerId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _context.ServerInvites.Add(invite);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            invite.Id,
            invite.Code,
            invite.ServerId,
            invite.CreatedAt,
            invite.ExpiresAt
        });
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinServer(
        JoinServerRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var code = request.Code
            .Trim()
            .ToUpper();

        var invite = await _context.ServerInvites
            .FirstOrDefaultAsync(i => i.Code == code);

        if (invite == null)
        {
            return NotFound("Invite findes ikke.");
        }

        if (invite.ExpiresAt < DateTime.UtcNow)
        {
            return BadRequest("Invite er udløbet.");
        }

        var alreadyMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == invite.ServerId &&
                sm.UserId == userId);

        if (alreadyMember)
        {
            return BadRequest(
                "Du er allerede medlem af serveren."
            );
        }

        var membership = new ServerMember
        {
            ServerId = invite.ServerId,
            UserId = userId,
            Role = "Member",
            JoinedAt = DateTime.UtcNow
        };

        _context.ServerMembers.Add(membership);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Du er nu medlem af serveren.",
            serverId = invite.ServerId
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvite(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var invite = await _context.ServerInvites
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invite == null)
        {
            return NotFound("Invite findes ikke.");
        }

        var membership = await _context.ServerMembers
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == invite.ServerId &&
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

        _context.ServerInvites.Remove(invite);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}