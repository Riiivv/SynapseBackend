using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class SendMessageRequest
{
    public int? ChannelId { get; set; }

    public int? PrivateChatId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string MessageContent { get; set; } = string.Empty;
}