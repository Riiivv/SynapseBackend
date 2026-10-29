namespace Synapse.Application.DTOs;

public class ServerResponse
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int OwnerId { get; set; }

    public UserResponse? Owner { get; set; }

    public DateTime CreatedAt { get; set; }
}