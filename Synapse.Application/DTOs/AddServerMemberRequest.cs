using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class AddServerMemberRequest
{
    [Range(1, int.MaxValue)]
    public int ServerId { get; set; }

    [Range(1, int.MaxValue)]
    public int UserId { get; set; }
}