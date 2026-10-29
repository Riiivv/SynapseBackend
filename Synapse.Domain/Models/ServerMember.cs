namespace Synapse.Domain.Models;

public class ServerMember
{
    public int Id { get; set; }

    public int ServerId { get; set; }
    public Server? Server { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public string Role { get; set; } = "Member";

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}