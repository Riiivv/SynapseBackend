namespace Synapse.Application.DTOs;

public class PrivateChatOverviewResponse
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<PrivateChatMemberInfoResponse> Members { get; set; }
        = new();
}