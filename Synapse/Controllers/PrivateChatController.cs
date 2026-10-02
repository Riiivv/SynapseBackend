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
public class PrivateChatsController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public PrivateChatsController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult> GetMyPrivateChats()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var chats = await _context.PrivateChats
            .Where(pc =>
                _context.PrivateChatMembers.Any(pm =>
                    pm.PrivateChatId == pc.Id &&
                    pm.UserId == userId))
            .Select(pc => new
            {
                pc.Id,
                pc.CreatedAt,

                Members = _context.PrivateChatMembers
                    .Where(pm => pm.PrivateChatId == pc.Id)
                    .Select(pm => new
                    {
                        pm.UserId,
                        Username = pm.User!.Username,
                        Email = pm.User.Email
                    })
                    .ToList()
            })
            .ToListAsync();

        return Ok(chats);
    }

    [HttpPost]
    public async Task<ActionResult<PrivateChat>> CreatePrivateChat(
        CreatePrivateChatRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        if (request.OtherUserId == currentUserId)
        {
            return BadRequest(
                "Du kan ikke oprette en privat chat med dig selv."
            );
        }

        var otherUserExists = await _context.Users
            .AnyAsync(u => u.Id == request.OtherUserId);

        if (!otherUserExists)
        {
            return NotFound("Brugeren findes ikke.");
        }

        var existingChat = await _context.PrivateChats
            .Where(pc =>
                _context.PrivateChatMembers.Any(pm =>
                    pm.PrivateChatId == pc.Id &&
                    pm.UserId == currentUserId)
                &&
                _context.PrivateChatMembers.Any(pm =>
                    pm.PrivateChatId == pc.Id &&
                    pm.UserId == request.OtherUserId)
            )
            .FirstOrDefaultAsync();

        if (existingChat != null)
        {
            return BadRequest(
                "Der findes allerede en privat chat mellem brugerne."
            );
        }

        var privateChat = new PrivateChat
        {
            CreatedAt = DateTime.UtcNow
        };

        _context.PrivateChats.Add(privateChat);

        await _context.SaveChangesAsync();

        var members = new List<PrivateChatMember>
        {
            new PrivateChatMember
            {
                PrivateChatId = privateChat.Id,
                UserId = currentUserId
            },

            new PrivateChatMember
            {
                PrivateChatId = privateChat.Id,
                UserId = request.OtherUserId
            }
        };

        _context.PrivateChatMembers.AddRange(members);

        await _context.SaveChangesAsync();

        return Ok(privateChat);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrivateChat>> GetPrivateChat(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var privateChat = await _context.PrivateChats
            .FirstOrDefaultAsync(pc => pc.Id == id);

        if (privateChat == null)
        {
            return NotFound("Private chat findes ikke.");
        }

        var isMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == id &&
                pm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        return Ok(privateChat);
    }

    [HttpGet("{id}/crypto-info")]
    public async Task<ActionResult<PrivateChatCryptoInfoResponse>> GetCryptoInfo(
        int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var currentUserId = int.Parse(userIdClaim.Value);

        var privateChat = await _context.PrivateChats
            .FirstOrDefaultAsync(pc => pc.Id == id);

        if (privateChat == null)
        {
            return NotFound("Private chat findes ikke.");
        }

        var currentUserIsMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == id &&
                pm.UserId == currentUserId);

        if (!currentUserIsMember)
        {
            return Forbid();
        }

        var members = await _context.PrivateChatMembers
            .Where(pm => pm.PrivateChatId == id)
            .Include(pm => pm.User)
            .ToListAsync();

        if (members.Count != 2)
        {
            return BadRequest(
                "E2EE crypto-info understøtter kun chats mellem 2 brugere."
            );
        }

        var otherMember = members
            .FirstOrDefault(pm => pm.UserId != currentUserId);

        if (otherMember?.User == null)
        {
            return NotFound("Den anden bruger findes ikke.");
        }

        if (string.IsNullOrWhiteSpace(otherMember.User.PublicKey))
        {
            return BadRequest(
                "Den anden bruger har ikke registreret en public key."
            );
        }

        var response = new PrivateChatCryptoInfoResponse
        {
            PrivateChatId = id,
            OtherUserId = otherMember.User.Id,
            Username = otherMember.User.Username,
            PublicKey = otherMember.User.PublicKey,

            PublicKeyFingerprint =
                otherMember.User.PublicKeyFingerprint
                ?? string.Empty,

            PublicKeyVersion =
                otherMember.User.PublicKeyVersion,

            PublicKeyUpdatedAt =
                otherMember.User.PublicKeyUpdatedAt
        };

        return Ok(response);
    }
}