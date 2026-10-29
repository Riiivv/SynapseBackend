using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class UpdateServerMemberRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}