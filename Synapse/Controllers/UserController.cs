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
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await _userService.GetAllAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var result = await _userService.GetByIdAsync(id);

        return HandleResult(result);
    }

    [HttpPut("public-key")]
    public async Task<IActionResult> UpdatePublicKey(
        UpdatePublicKeyRequest request)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var result =
            await _userService.UpdatePublicKeyAsync(
                userId.Value,
                request);

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

    [HttpGet("{id}/public-key")]
    public async Task<ActionResult<UserPublicKeyResponse>>
        GetPublicKey(int id)
    {
        var result =
            await _userService.GetPublicKeyAsync(id);

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