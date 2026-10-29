using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IPrivateChatMemberRepository
{
    Task<List<PrivateChatMember>> GetByChatIdAsync(int privateChatId);

    Task<PrivateChatMember?> GetMembershipAsync(
        int privateChatId,
        int userId);

    Task<bool> IsMemberAsync(
        int privateChatId,
        int userId);

    Task AddAsync(PrivateChatMember member);

    Task DeleteAsync(PrivateChatMember member);

    Task SaveChangesAsync();
}