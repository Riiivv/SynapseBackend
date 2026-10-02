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
public class PrivateChatMembersController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public PrivateChatMembersController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult<PrivateChatMemberResponse>> AddMember(
        AddPrivateChatMemberRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var chatExists = await _context.PrivateChats
            .AnyAsync(pc => pc.Id == request.PrivateChatId);

        if (!chatExists)
        {
            return NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == request.PrivateChatId &&
                pm.UserId == currentUserId);

        if (!currentUserIsMember)
        {
            return Forbid();
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId);

        if (user == null)
        {
            return NotFound("Bruger findes ikke.");
        }

        var alreadyMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == request.PrivateChatId &&
                pm.UserId == request.UserId);

        if (alreadyMember)
        {
            return BadRequest("Brugeren er allerede medlem af chatten.");
        }

        var privateChatMember = new PrivateChatMember
        {
            PrivateChatId = request.PrivateChatId,
            UserId = request.UserId
        };

        _context.PrivateChatMembers.Add(privateChatMember);

        await _context.SaveChangesAsync();

        var response = new PrivateChatMemberResponse
        {
            Id = privateChatMember.Id,
            PrivateChatId = privateChatMember.PrivateChatId,
            UserId = user.Id,

            User = new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            }
        };

        return Ok(response);
    }

    [HttpGet("chat/{privateChatId}")]
    public async Task<ActionResult<List<PrivateChatMemberResponse>>> GetMembers(
        int privateChatId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var chatExists = await _context.PrivateChats
            .AnyAsync(pc => pc.Id == privateChatId);

        if (!chatExists)
        {
            return NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == currentUserId);

        if (!currentUserIsMember)
        {
            return Forbid();
        }

        var members = await _context.PrivateChatMembers
            .Where(pm => pm.PrivateChatId == privateChatId)
            .Select(pm => new PrivateChatMemberResponse
            {
                Id = pm.Id,
                PrivateChatId = pm.PrivateChatId,
                UserId = pm.UserId,

                User = pm.User == null
                    ? null
                    : new UserResponse
                    {
                        Id = pm.User.Id,
                        Username = pm.User.Username,
                        Email = pm.User.Email
                    }
            })
            .ToListAsync();

        return Ok(members);
    }

    [HttpDelete("chat/{privateChatId}/leave")]
    public async Task<IActionResult> LeavePrivateChat(int privateChatId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var privateChat = await _context.PrivateChats
            .FirstOrDefaultAsync(pc => pc.Id == privateChatId);

        if (privateChat == null)
        {
            return NotFound("Private chat findes ikke.");
        }

        var membership = await _context.PrivateChatMembers
            .FirstOrDefaultAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == userId);

        if (membership == null)
        {
            return BadRequest("Du er ikke medlem af chatten.");
        }

        _context.PrivateChatMembers.Remove(membership);

        await _context.SaveChangesAsync();

        var hasMembers = await _context.PrivateChatMembers
            .AnyAsync(pm => pm.PrivateChatId == privateChatId);

        if (!hasMembers)
        {
            var messages = await _context.Messages
                .Where(m => m.PrivateChatId == privateChatId)
                .ToListAsync();

            _context.Messages.RemoveRange(messages);
            _context.PrivateChats.Remove(privateChat);

            await _context.SaveChangesAsync();
        }

        return NoContent();
    }

    [HttpDelete("chat/{privateChatId}/user/{userId}")]
    public async Task<IActionResult> RemoveMember(
        int privateChatId,
        int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var chatExists = await _context.PrivateChats
            .AnyAsync(pc => pc.Id == privateChatId);

        if (!chatExists)
        {
            return NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == currentUserId);

        if (!currentUserIsMember)
        {
            return Forbid();
        }

        var membership = await _context.PrivateChatMembers
            .FirstOrDefaultAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == userId);

        if (membership == null)
        {
            return NotFound("Brugeren er ikke medlem af chatten.");
        }

        _context.PrivateChatMembers.Remove(membership);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}