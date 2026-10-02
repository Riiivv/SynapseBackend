namespace Synapse.Backend.DTOs;

public class ServerMemberResponse
{
    public int Id { get; set; }

    public int ServerId { get; set; }

    public int UserId { get; set; }

    public string Role { get; set; } = string.Empty;

    public DateTime JoinedAt { get; set; }

    public UserResponse? User { get; set; }
}