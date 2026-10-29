using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class ServerMemberRepository : IServerMemberRepository
{
    private readonly SynapseDbContext _context;

    public ServerMemberRepository(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<List<ServerMember>> GetByServerIdAsync(int serverId)
    {
        return await _context.ServerMembers
            .Where(sm => sm.ServerId == serverId)
            .Include(sm => sm.User)
            .ToListAsync();
    }

    public async Task<ServerMember?> GetMembershipAsync(
        int serverId,
        int userId)
    {
        return await _context.ServerMembers
            .Include(sm => sm.User)
            .FirstOrDefaultAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == userId);
    }

    public async Task<bool> IsMemberAsync(
        int serverId,
        int userId)
    {
        return await _context.ServerMembers
            .AnyAsync(sm =>
                sm.ServerId == serverId &&
                sm.UserId == userId);
    }

    public async Task AddAsync(ServerMember member)
    {
        await _context.ServerMembers.AddAsync(member);
    }

    public Task UpdateAsync(ServerMember member)
    {
        _context.ServerMembers.Update(member);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ServerMember member)
    {
        _context.ServerMembers.Remove(member);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}