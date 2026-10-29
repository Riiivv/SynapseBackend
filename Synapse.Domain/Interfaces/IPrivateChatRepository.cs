using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IPrivateChatRepository
{
    Task<PrivateChat?> GetByIdAsync(int id);

    Task<List<PrivateChat>> GetForUserAsync(int userId);

    Task<PrivateChat?> GetBetweenUsersAsync(
        int firstUserId,
        int secondUserId);

    Task AddAsync(PrivateChat privateChat);

    Task DeleteAsync(PrivateChat privateChat);

    Task SaveChangesAsync();
}