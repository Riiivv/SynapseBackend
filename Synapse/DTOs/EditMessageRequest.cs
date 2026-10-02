using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class EditMessageRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(2000)]
    public string MessageContent { get; set; } = string.Empty;
}