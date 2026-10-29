using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class CreateChannelRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ServerId { get; set; }
}