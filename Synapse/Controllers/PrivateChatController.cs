using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Synapse.Application.Common;
using Synapse.Application.DTOs;
using Synapse.Application.Interfaces;
using System.Security.Claims;

namespace Synapse.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PrivateChatsController : ControllerBase
{
    private readonly IPrivateChatService _privateChatService;

    public PrivateChatsController(
        IPrivateChatService privateChatService)
    {
        _privateChatService = privateChatService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PrivateChatOverviewResponse>>>
        GetMyPrivateChats()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var chats = await _privateChatService
            .GetMyChatsAsync(userId.Value);

        return Ok(chats);
    }

    [HttpPost]
    public async Task<ActionResult<PrivateChatResponse>>
        CreatePrivateChat(
            CreatePrivateChatRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _privateChatService
            .CreateAsync(
                userId.Value,
                request);

        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrivateChatResponse>>
        GetPrivateChat(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _privateChatService
            .GetByIdAsync(
                id,
                userId.Value);

        return HandleResult(result);
    }

    [HttpGet("{id}/crypto-info")]
    public async Task<ActionResult<PrivateChatCryptoInfoResponse>>
        GetCryptoInfo(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _privateChatService
            .GetCryptoInfoAsync(
                id,
                userId.Value);

        return HandleResult(result);
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