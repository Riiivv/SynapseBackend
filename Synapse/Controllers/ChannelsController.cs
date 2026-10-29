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
public class ChannelsController : ControllerBase
{
    private readonly IChannelService _channelService;

    public ChannelsController(IChannelService channelService)
    {
        _channelService = channelService;
    }

    [HttpGet("server/{serverId}")]
    public async Task<ActionResult<List<ChannelResponse>>> GetServerChannels(
        int serverId)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _channelService
            .GetServerChannelsAsync(serverId, userId.Value);

        return result.Status switch
        {
            ServiceStatus.Success => Ok(result.Data),
            ServiceStatus.NotFound => NotFound(result.Error),
            ServiceStatus.Forbidden => Forbid(),
            ServiceStatus.BadRequest => BadRequest(result.Error),
            _ => BadRequest()
        };
    }

    [HttpPost]
    public async Task<ActionResult<ChannelResponse>> CreateChannel(
        CreateChannelRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _channelService
            .CreateAsync(userId.Value, request);

        return result.Status switch
        {
            ServiceStatus.Success => Ok(result.Data),
            ServiceStatus.NotFound => NotFound(result.Error),
            ServiceStatus.Forbidden => Forbid(),
            ServiceStatus.BadRequest => BadRequest(result.Error),
            _ => BadRequest()
        };
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ChannelResponse>> UpdateChannel(
        int id,
        UpdateChannelRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _channelService
            .UpdateAsync(id, userId.Value, request);

        return result.Status switch
        {
            ServiceStatus.Success => Ok(result.Data),
            ServiceStatus.NotFound => NotFound(result.Error),
            ServiceStatus.Forbidden => Forbid(),
            ServiceStatus.BadRequest => BadRequest(result.Error),
            _ => BadRequest()
        };
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteChannel(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _channelService
            .DeleteAsync(id, userId.Value);

        return result.Status switch
        {
            ServiceStatus.Success => NoContent(),
            ServiceStatus.NotFound => NotFound(result.Error),
            ServiceStatus.Forbidden => Forbid(),
            ServiceStatus.BadRequest => BadRequest(result.Error),
            _ => BadRequest()
        };
    }

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirst(
            ClaimTypes.NameIdentifier);

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