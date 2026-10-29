using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class CreateServerInviteRequest
{
    [Range(1, int.MaxValue)]
    public int ServerId { get; set; }
}