namespace Synapse.Domain.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? PublicKey { get; set; }

    public string? PublicKeyFingerprint { get; set; }

    public int PublicKeyVersion { get; set; } = 0;

    public DateTime? PublicKeyUpdatedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}