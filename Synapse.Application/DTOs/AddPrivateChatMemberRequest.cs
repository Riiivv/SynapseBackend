using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class AddPrivateChatMemberRequest
{
    [Range(1, int.MaxValue)]
    public int PrivateChatId { get; set; }

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}