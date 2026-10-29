using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IServerRepository
{
    Task<List<Server>> GetAllAsync();

    Task<Server?> GetByIdAsync(int id);

    Task<List<Server>> GetServersForUserAsync(int userId);

    Task AddAsync(Server server);

    Task UpdateAsync(Server server);

    Task DeleteAsync(Server server);

    Task SaveChangesAsync();
}