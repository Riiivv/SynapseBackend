namespace Synapse.Domain.Models;

public class Server
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public User? Owner { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}