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
public class ServerMembersController : ControllerBase
{
    private readonly IServerMemberService _serverMemberService;

    public ServerMembersController(
        IServerMemberService serverMemberService)
    {
        _serverMemberService = serverMemberService;
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<ServerMemberResponse>>> GetServerMembers(
        int serverId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverMemberService.GetServerMembersAsync(
                serverId,
                userId.Value);

        return HandleResult(result);
    }

    [HttpPost]
    public async Task<ActionResult<ServerMemberResponse>> AddMember(
        AddServerMemberRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverMemberService.AddMemberAsync(
                userId.Value,
                request);

        return HandleResult(result);
    }

    [HttpPut("server/{serverId}/user/{userId}/role")]
    public async Task<ActionResult<ServerMemberResponse>> UpdateRole(
        int serverId,
        int userId,
        UpdateServerMemberRoleRequest request)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverMemberService.UpdateRoleAsync(
                serverId,
                userId,
                currentUserId.Value,
                request);

        return HandleResult(result);
    }

    [HttpDelete("server/{serverId}/user/{userId}")]
    public async Task<IActionResult> RemoveMember(
        int serverId,
        int userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverMemberService.RemoveMemberAsync(
                serverId,
                userId,
                currentUserId.Value);

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

    [HttpDelete("server/{serverId}/leave")]
    public async Task<IActionResult> LeaveServer(
        int serverId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverMemberService.LeaveServerAsync(
                serverId,
                currentUserId.Value);

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