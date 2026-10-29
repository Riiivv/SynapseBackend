using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly SynapseDbContext _context;

    public ChannelRepository(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<Channel?> GetByIdAsync(int id)
    {
        return await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Channel>> GetByServerIdAsync(int serverId)
    {
        return await _context.Channels
            .Where(c => c.ServerId == serverId)
            .ToListAsync();
    }

    public async Task AddAsync(Channel channel)
    {
        await _context.Channels.AddAsync(channel);
    }

    public Task UpdateAsync(Channel channel)
    {
        _context.Channels.Update(channel);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Channel channel)
    {
        _context.Channels.Remove(channel);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}