using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IMessageRepository
{
    Task<Message?> GetByIdAsync(int id);

    Task<List<Message>> GetChannelMessagesAsync(int channelId);

    Task<List<Message>> GetPrivateMessagesAsync(int privateChatId);

    Task AddAsync(Message message);

    Task UpdateAsync(Message message);

    Task DeleteAsync(Message message);

    Task SaveChangesAsync();
}