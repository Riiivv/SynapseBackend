using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class JoinServerRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}