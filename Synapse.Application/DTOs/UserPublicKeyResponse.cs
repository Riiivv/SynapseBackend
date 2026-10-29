namespace Synapse.Application.DTOs;

public class UserPublicKeyResponse
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PublicKey { get; set; } = string.Empty;

    public string PublicKeyFingerprint { get; set; } = string.Empty;

    public int PublicKeyVersion { get; set; }

    public DateTime? PublicKeyUpdatedAt { get; set; }
}