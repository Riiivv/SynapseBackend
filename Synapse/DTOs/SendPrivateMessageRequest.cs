using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class SendPrivateMessageRequest
{
    [Range(1, int.MaxValue)]
    public int PrivateChatId { get; set; }

    [Required]
    public string Ciphertext { get; set; } = string.Empty;

    [Required]
    public string Nonce { get; set; } = string.Empty;

    [Required]
    public string EncryptedKeyForSender { get; set; } = string.Empty;

    [Required]
    public string EncryptedKeyForRecipient { get; set; } = string.Empty;
}