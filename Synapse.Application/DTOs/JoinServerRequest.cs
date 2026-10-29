using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class JoinServerRequest
{
    [Required]
    public string Code { get; set; } = string.Empty;
}