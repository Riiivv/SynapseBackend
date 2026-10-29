using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using Synapse.Backend.Hubs;
using System.Security.Claims;

namespace Synapse.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageService _messageService;
    private readonly IHubContext<ChatHub> _hub;

    public MessagesController(
        IMessageService messageService,
        IHubContext<ChatHub> hub)
    {
        _messageService = messageService;
        _hub = hub;
    }

    [HttpGet("channel/{channelId}")]
    public async Task<ActionResult<List<MessageResponse>>> GetChannelMessages(
        int channelId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _messageService.GetChannelMessagesAsync(
                channelId,
                userId.Value);

        return HandleResult(result);
    }

    [HttpGet("private/{privateChatId}")]
    public async Task<ActionResult<List<MessageResponse>>> GetPrivateMessages(
        int privateChatId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _messageService.GetPrivateMessagesAsync(
                privateChatId,
                userId.Value);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<MessageResponse>> SendChannelMessage(
        SendMessageRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _messageService.SendChannelMessageAsync(
                userId.Value,
                request);

        if (result.Status != ServiceStatus.Success)
        {
            return HandleResult(result);
        }

        var message = result.Data!;

        await _hub.Clients
            .Group($"channel-{message.ChannelId}")
            .SendAsync(
                "ReceiveMessage",
                message.UserId,
                message.MessageContent);

        return Ok(message);
    }

    [HttpPost("private")]
    public async Task<ActionResult<MessageResponse>> SendPrivateMessage(
        SendPrivateMessageRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _messageService.SendPrivateMessageAsync(
                userId.Value,
                request);

        if (result.Status != ServiceStatus.Success)
        {
            return HandleResult(result);
        }

        var message = result.Data!;

        await _hub.Clients
            .Group($"private-{message.PrivateChatId}")
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
                });

        return Ok(message);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MessageResponse>> UpdateMessage(
        int id,
        EditMessageRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _messageService.UpdateMessageAsync(
                id,
                userId.Value,
                request);

        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _messageService.DeleteMessageAsync(
                id,
                userId.Value);

        return result.Status switch
        {
            ServiceStatus.Success =>
                NoContent(),

            ServiceStatus.NotFound =>
                NotFound(result.Error),

            ServiceStatus.Forbidden =>
                Forbid(),

            ServiceStatus.BadRequest =>
                BadRequest(result.Error),

            _ => BadRequest()
        };
    }

    private ActionResult<T> HandleResult<T>(
        ServiceResult<T> result)
    {
        return result.Status switch
        {
            ServiceStatus.Success =>
                Ok(result.Data),

            ServiceStatus.NotFound =>
                NotFound(result.Error),

            ServiceStatus.Forbidden =>
                Forbid(),

            ServiceStatus.BadRequest =>
                BadRequest(result.Error),

            _ => BadRequest()
        };
    }

    private int? GetCurrentUserId()
    {
        var claim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            return null;
        }

        if (!int.TryParse(
                claim.Value,
                out var userId))
        {
            return null;
        }

        return userId;
    }
}