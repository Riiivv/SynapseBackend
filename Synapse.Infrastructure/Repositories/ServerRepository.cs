using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class ServerRepository : IServerRepository
{
    private readonly SynapseDbContext _context;

    public ServerRepository(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<List<Server>> GetAllAsync()
    {
        return await _context.Servers
            .ToListAsync();
    }

    public async Task<Server?> GetByIdAsync(int id)
    {
        return await _context.Servers
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Server>> GetServersForUserAsync(int userId)
    {
        return await _context.ServerMembers
            .Where(sm => sm.UserId == userId)
            .Select(sm => sm.Server!)
            .ToListAsync();
    }

    public async Task AddAsync(Server server)
    {
        await _context.Servers.AddAsync(server);
    }

    public Task UpdateAsync(Server server)
    {
        _context.Servers.Update(server);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Server server)
    {
        _context.Servers.Remove(server);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}