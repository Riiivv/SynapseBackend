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
public class PrivateChatMembersController : ControllerBase
{
    private readonly IPrivateChatMemberService _privateChatMemberService;

    public PrivateChatMembersController(
        IPrivateChatMemberService privateChatMemberService)
    {
        _privateChatMemberService = privateChatMemberService;
    }

    [HttpPost]
    public async Task<ActionResult<PrivateChatMemberResponse>> AddMember(
        AddPrivateChatMemberRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _privateChatMemberService.AddMemberAsync(
                userId.Value,
                request);

        return HandleResult(result);
    }

    [HttpGet("chat/{privateChatId}")]
    public async Task<ActionResult<List<PrivateChatMemberResponse>>> GetMembers(
        int privateChatId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _privateChatMemberService.GetMembersAsync(
                privateChatId,
                userId.Value);

        return HandleResult(result);
    }

    [HttpDelete("chat/{privateChatId}/leave")]
    public async Task<IActionResult> LeavePrivateChat(
        int privateChatId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _privateChatMemberService.LeavePrivateChatAsync(
                privateChatId,
                userId.Value);

        return HandleBoolResult(result);
    }

    [HttpDelete("chat/{privateChatId}/user/{userId}")]
    public async Task<IActionResult> RemoveMember(
        int privateChatId,
        int userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result =
            await _privateChatMemberService.RemoveMemberAsync(
                privateChatId,
                userId,
                currentUserId.Value);

        return HandleBoolResult(result);
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

    private IActionResult HandleBoolResult(
        ServiceResult<bool> result)
    {
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