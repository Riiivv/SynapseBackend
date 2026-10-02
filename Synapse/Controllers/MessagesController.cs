using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Synapse.Backend.Data;
using Synapse.Backend.DTOs;
using Synapse.Backend.Hubs;
using Synapse.Backend.Models;
using System.Security.Claims;

namespace Synapse.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly SynapseDbContext _context;
    private readonly IHubContext<ChatHub> _hub;

    public MessagesController(
        SynapseDbContext context,
        IHubContext<ChatHub> hub)
    {
        _context = context;
        _hub = hub;
    }

    // =========================
    // GET CHANNEL MESSAGES
    // =========================

    [HttpGet("channel/{channelId}")]
    public async Task<ActionResult<List<MessageResponse>>> GetChannelMessages(
        int channelId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == channelId);

        if (channel == null)
        {
            return NotFound("Channel findes ikke.");
        }

        var isMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == channel.ServerId &&
                sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var messages = await _context.Messages
            .Where(m => m.ChannelId == channelId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                UserId = m.UserId,

                User = m.User == null
                    ? null
                    : new UserResponse
                    {
                        Id = m.User.Id,
                        Username = m.User.Username,
                        Email = m.User.Email
                    },

                ChannelId = m.ChannelId,
                PrivateChatId = null,

                MessageContent = m.MessageContent,

                Ciphertext = null,
                Nonce = null,
                EncryptedKey = null,

                SentAt = m.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    // =========================
    // GET PRIVATE E2EE MESSAGES
    // =========================

    [HttpGet("private/{privateChatId}")]
    public async Task<ActionResult<List<MessageResponse>>> GetPrivateMessages(
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

        var isMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == currentUserId);

        if (!isMember)
        {
            return Forbid();
        }

        var messages = await _context.Messages
            .Where(m => m.PrivateChatId == privateChatId)
            .OrderBy(m => m.SentAt)
            .Select(m => new MessageResponse
            {
                Id = m.Id,
                UserId = m.UserId,

                User = m.User == null
                    ? null
                    : new UserResponse
                    {
                        Id = m.User.Id,
                        Username = m.User.Username,
                        Email = m.User.Email
                    },

                ChannelId = null,
                PrivateChatId = m.PrivateChatId,

                // Private chats returnerer aldrig plaintext
                MessageContent = null,

                Ciphertext = m.Ciphertext,
                Nonce = m.Nonce,

                // Sender får sender-key.
                // Modtager får recipient-key.
                EncryptedKey =
                    m.UserId == currentUserId
                        ? m.EncryptedKeyForSender
                        : m.EncryptedKeyForRecipient,

                SentAt = m.SentAt
            })
            .ToListAsync();

        return Ok(messages);
    }

    // =========================
    // SEND CHANNEL MESSAGE
    // =========================

    [HttpPost]
    public async Task<ActionResult<MessageResponse>> SendChannelMessage(
        SendMessageRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        if (request.ChannelId == null)
        {
            return BadRequest(
                "Dette endpoint bruges kun til channel-beskeder."
            );
        }

        if (request.PrivateChatId != null)
        {
            return BadRequest(
                "Private beskeder skal sendes krypteret via /api/Messages/private."
            );
        }

        if (string.IsNullOrWhiteSpace(request.MessageContent))
        {
            return BadRequest("Beskeden må ikke være tom.");
        }

        var channel = await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == request.ChannelId.Value);

        if (channel == null)
        {
            return NotFound("Channel findes ikke.");
        }

        var isMember = await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == channel.ServerId &&
                sm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var message = new Message
        {
            UserId = userId,
            ChannelId = request.ChannelId,
            PrivateChatId = null,

            MessageContent = request.MessageContent,

            Ciphertext = null,
            Nonce = null,
            EncryptedKeyForSender = null,
            EncryptedKeyForRecipient = null,

            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        await _hub.Clients
            .Group($"channel-{message.ChannelId}")
            .SendAsync(
                "ReceiveMessage",
                message.UserId,
                message.MessageContent
            );

        var response = new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,
            ChannelId = message.ChannelId,
            PrivateChatId = null,
            MessageContent = message.MessageContent,
            Ciphertext = null,
            Nonce = null,
            EncryptedKey = null,
            SentAt = message.SentAt
        };

        return Ok(response);
    }

    // =========================
    // SEND PRIVATE E2EE MESSAGE
    // =========================

    [HttpPost("private")]
    public async Task<ActionResult<MessageResponse>> SendPrivateMessage(
        SendPrivateMessageRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var chatExists = await _context.PrivateChats
            .AnyAsync(pc => pc.Id == request.PrivateChatId);

        if (!chatExists)
        {
            return NotFound("Private chat findes ikke.");
        }

        var isMember = await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == request.PrivateChatId &&
                pm.UserId == userId);

        if (!isMember)
        {
            return Forbid();
        }

        var memberCount = await _context.PrivateChatMembers
            .CountAsync(pm =>
                pm.PrivateChatId == request.PrivateChatId);

        if (memberCount != 2)
        {
            return BadRequest(
                "Denne E2EE-implementering understøtter kun private chats mellem 2 brugere."
            );
        }

        var message = new Message
        {
            UserId = userId,

            ChannelId = null,
            PrivateChatId = request.PrivateChatId,

            // Serveren får aldrig plaintext
            MessageContent = null,

            Ciphertext = request.Ciphertext,
            Nonce = request.Nonce,

            EncryptedKeyForSender =
                request.EncryptedKeyForSender,

            EncryptedKeyForRecipient =
                request.EncryptedKeyForRecipient,

            SentAt = DateTime.UtcNow
        };

        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        await _hub.Clients
            .Group($"private-{request.PrivateChatId}")
            .SendAsync(
                "ReceivePrivateMessage",
                new
                {
                    message.Id,
                    message.UserId,
                    message.PrivateChatId,
                    message.Ciphertext,
                    message.Nonce,
                    message.SentAt
                }
            );

        var response = new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,
            ChannelId = null,
            PrivateChatId = message.PrivateChatId,

            MessageContent = null,

            Ciphertext = message.Ciphertext,
            Nonce = message.Nonce,

            // Senderen får sin egen encrypted key tilbage
            EncryptedKey = message.EncryptedKeyForSender,

            SentAt = message.SentAt
        };

        return Ok(response);
    }

    // =========================
    // EDIT CHANNEL MESSAGE
    // =========================

    [HttpPut("{id}")]
    public async Task<ActionResult<MessageResponse>> UpdateMessage(
        int id,
        EditMessageRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
        {
            return NotFound("Besked findes ikke.");
        }

        if (message.UserId != userId)
        {
            return Forbid();
        }

        // E2EE private messages redigeres ikke via plaintext endpoint
        if (message.PrivateChatId != null)
        {
            return BadRequest(
                "Krypterede private beskeder kan ikke redigeres via dette endpoint."
            );
        }

        if (string.IsNullOrWhiteSpace(request.MessageContent))
        {
            return BadRequest("Beskeden må ikke være tom.");
        }

        message.MessageContent = request.MessageContent;

        await _context.SaveChangesAsync();

        var response = new MessageResponse
        {
            Id = message.Id,
            UserId = message.UserId,
            ChannelId = message.ChannelId,
            PrivateChatId = null,
            MessageContent = message.MessageContent,
            Ciphertext = null,
            Nonce = null,
            EncryptedKey = null,
            SentAt = message.SentAt
        };

        return Ok(response);
    }

    // =========================
    // DELETE MESSAGE
    // =========================

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
        {
            return NotFound("Besked findes ikke.");
        }

        if (message.UserId != userId)
        {
            return Forbid();
        }

        _context.Messages.Remove(message);

        await _context.SaveChangesAsync();

        return NoContent();
    }
}