using System.ComponentModel.DataAnnotations;

namespace Synapse.Backend.DTOs;

public class UpdatePublicKeyRequest
{
    [Required]
    public string PublicKey { get; set; } = string.Empty;
}