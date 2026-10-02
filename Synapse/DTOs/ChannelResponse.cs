namespace Synapse.Backend.DTOs;

public class ChannelResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int ServerId { get; set; }

    public DateTime CreatedAt { get; set; }
}