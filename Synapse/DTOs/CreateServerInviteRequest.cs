using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class CreateServerInviteRequest
{
    [Range(1, int.MaxValue)]
    public int ServerId { get; set; }
}