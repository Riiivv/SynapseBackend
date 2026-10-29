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
public class ServersController : ControllerBase
{
    private readonly IServerService _serverService;

    public ServersController(IServerService serverService)
    {
        _serverService = serverService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ServerResponse>>> GetServers()
    {
        var servers = await _serverService.GetAllAsync();

        return Ok(servers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServerResponse>> GetServer(int id)
    {
        var server = await _serverService.GetByIdAsync(id);

        if (server == null)
        {
            return NotFound("Server findes ikke.");
        }

        return Ok(server);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<ServerResponse>>> GetMyServers()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var servers =
            await _serverService.GetForUserAsync(userId.Value);

        return Ok(servers);
    }

    [HttpPost]
    public async Task<ActionResult<ServerResponse>> CreateServer(
        CreateServerRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var server = await _serverService.CreateAsync(
            userId.Value,
            request);

        return CreatedAtAction(
            nameof(GetServer),
            new { id = server.Id },
            server);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ServerResponse>> UpdateServer(
        int id,
        UpdateServerRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _serverService.UpdateAsync(
            id,
            userId.Value,
            request);

        return result.Status switch
        {
            ServiceStatus.Success => Ok(result.Data),

            ServiceStatus.NotFound =>
                NotFound(result.Error),

            ServiceStatus.Forbidden =>
                Forbid(),

            ServiceStatus.BadRequest =>
                BadRequest(result.Error),

            _ => BadRequest()
        };
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServer(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _serverService.DeleteAsync(
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

    private int? GetCurrentUserId()
    {
        var claim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null)
        {
            return null;
        }

        if (!int.TryParse(claim.Value, out var userId))
        {
            return null;
        }

        return userId;
    }
}