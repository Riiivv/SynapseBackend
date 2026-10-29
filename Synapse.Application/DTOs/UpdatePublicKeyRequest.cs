using System.ComponentModel.DataAnnotations;

namespace Synapse.Application.DTOs;

public class UpdatePublicKeyRequest
{
    [Required]
    public string PublicKey { get; set; } = string.Empty;
}