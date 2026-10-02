using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class CreatePrivateChatRequest
{
    [Range(1, int.MaxValue)]
    public int OtherUserId { get; set; }
}