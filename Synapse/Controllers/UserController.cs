using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Synapse.Backend.Data;
using Synapse.Backend.DTOs;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Synapse.Backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly SynapseDbContext _context;

    public UsersController(SynapseDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetUsers()
    {
        var users = await _context.Users
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound("Bruger findes ikke.");
        }

        return Ok(user);
    }

    [HttpPut("public-key")]
    public async Task<IActionResult> UpdatePublicKey(
        UpdatePublicKeyRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        var userId = int.Parse(userIdClaim.Value);

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("Bruger findes ikke.");
        }

        var publicKey = request.PublicKey.Trim();

        if (string.IsNullOrWhiteSpace(publicKey))
        {
            return BadRequest("Public key må ikke være tom.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(publicKey);

        var hash = SHA256.HashData(keyBytes);

        var fingerprint = Convert.ToHexString(hash);

        // Hvis samme key allerede er registreret,
        // behøver vi ikke lave en ny version.
        if (user.PublicKeyFingerprint == fingerprint)
        {
            return NoContent();
        }

        user.PublicKey = publicKey;
        user.PublicKeyFingerprint = fingerprint;
        user.PublicKeyVersion += 1;
        user.PublicKeyUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/public-key")]
    public async Task<IActionResult> GetPublicKey(int id)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound("Bruger findes ikke.");
        }

        if (string.IsNullOrWhiteSpace(user.PublicKey))
        {
            return NotFound(
                "Brugeren har ikke registreret en public key."
            );
        }

        return Ok(new
        {
            user.Id,
            user.Username,
            user.PublicKey,
            user.PublicKeyFingerprint,
            user.PublicKeyVersion,
            user.PublicKeyUpdatedAt
        });
    }
}