namespace Synapse.Backend.Models;

public class Message
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int? ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public int? PrivateChatId { get; set; }
    public PrivateChat? PrivateChat { get; set; }

    // Bruges til normale server/channel-beskeder
    public string? MessageContent { get; set; }

    // Bruges til E2EE private beskeder
    public string? Ciphertext { get; set; }

    public string? Nonce { get; set; }

    public string? EncryptedKeyForSender { get; set; }

    public string? EncryptedKeyForRecipient { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}