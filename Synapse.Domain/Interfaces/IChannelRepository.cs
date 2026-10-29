using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IChannelRepository
{
    Task<Channel?> GetByIdAsync(int id);

    Task<List<Channel>> GetByServerIdAsync(int serverId);

    Task AddAsync(Channel channel);

    Task UpdateAsync(Channel channel);

    Task DeleteAsync(Channel channel);

    Task SaveChangesAsync();
}