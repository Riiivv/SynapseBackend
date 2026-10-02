namespace Synapse.Backend.Models;

public class Channel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int ServerId { get; set; }

    public Server? Server { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}