using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IServerInviteRepository
{
    Task<ServerInvite?> GetByIdAsync(int id);

    Task<ServerInvite?> GetByCodeAsync(string code);

    Task<List<ServerInvite>> GetByServerIdAsync(int serverId);

    Task AddAsync(ServerInvite invite);

    Task DeleteAsync(ServerInvite invite);

    Task SaveChangesAsync();
}