using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class ServerInviteRepository : IServerInviteRepository
{
    private readonly SynapseDbContext _context;

    public ServerInviteRepository(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<ServerInvite?> GetByIdAsync(int id)
    {
        return await _context.ServerInvites
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<ServerInvite?> GetByCodeAsync(string code)
    {
        return await _context.ServerInvites
            .FirstOrDefaultAsync(i => i.Code == code);
    }

    public async Task<List<ServerInvite>> GetByServerIdAsync(int serverId)
    {
        return await _context.ServerInvites
            .Where(i => i.ServerId == serverId)
            .ToListAsync();
    }

    public async Task AddAsync(ServerInvite invite)
    {
        await _context.ServerInvites.AddAsync(invite);
    }

    public Task DeleteAsync(ServerInvite invite)
    {
        _context.ServerInvites.Remove(invite);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}