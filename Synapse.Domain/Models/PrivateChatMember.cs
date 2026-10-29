namespace Synapse.Domain.Models;

public class PrivateChatMember
{
    public int Id { get; set; }

    public int PrivateChatId { get; set; }

    public PrivateChat? PrivateChat { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }
}