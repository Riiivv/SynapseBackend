using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class CreateServerRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
}