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
public class ServerInvitesController : ControllerBase
{
    private readonly IServerInviteService _serverInviteService;

    public ServerInvitesController(
        IServerInviteService serverInviteService)
    {
        _serverInviteService = serverInviteService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvite(
        CreateServerInviteRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverInviteService.CreateInviteAsync(
                userId.Value,
                request.ServerId);

        return HandleObjectResult(result);
    }

    [HttpPost("join")]
    public async Task<IActionResult> JoinServer(
        JoinServerRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverInviteService.JoinServerAsync(
                userId.Value,
                request.Code);

        return HandleObjectResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInvite(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _serverInviteService.DeleteInviteAsync(
                userId.Value,
                id);

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

    private IActionResult HandleObjectResult(
        ServiceResult<object> result)
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