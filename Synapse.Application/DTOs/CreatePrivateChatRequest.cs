using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class CreatePrivateChatRequest
{
    [Range(1, int.MaxValue)]
    public int OtherUserId { get; set; }
}