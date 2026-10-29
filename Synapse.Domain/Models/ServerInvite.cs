namespace Synapse.Domain.Models;

public class ServerInvite
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public int ServerId { get; set; }
    public Server? Server { get; set; }

    public int CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAt { get; set; }
}