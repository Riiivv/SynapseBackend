namespace Synapse.Application.DTOs;

public class PrivateChatMemberResponse
{
    public int Id { get; set; }

    public int PrivateChatId { get; set; }

    public int UserId { get; set; }

    public UserResponse? User { get; set; }
}