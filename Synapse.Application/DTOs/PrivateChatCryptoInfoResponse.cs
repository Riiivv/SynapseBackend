namespace Synapse.Application.DTOs;

public class PrivateChatCryptoInfoResponse
{
    public int PrivateChatId { get; set; }

    public int OtherUserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PublicKey { get; set; } = string.Empty;

    public string PublicKeyFingerprint { get; set; } = string.Empty;

    public int PublicKeyVersion { get; set; }

    public DateTime? PublicKeyUpdatedAt { get; set; }
}