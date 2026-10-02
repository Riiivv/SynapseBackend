using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class UpdateServerMemberRoleRequest
{
    [Required]
    public string Role { get; set; } = string.Empty;
}