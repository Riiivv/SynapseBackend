namespace Synapse.Backend.Models;

public class PrivateChat
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}