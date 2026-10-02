namespace Synapse.Backend.DTOs;

public class MessageResponse
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public UserResponse? User { get; set; }

    public int? ChannelId { get; set; }

    public int? PrivateChatId { get; set; }

    // Channel message
    public string? MessageContent { get; set; }

    // E2EE private message
    public string? Ciphertext { get; set; }

    public string? Nonce { get; set; }

    public string? EncryptedKey { get; set; }

    public DateTime SentAt { get; set; }
}