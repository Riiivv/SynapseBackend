using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class PrivateChatMemberRepository
    : IPrivateChatMemberRepository
{
    private readonly SynapseDbContext _context;

    public PrivateChatMemberRepository(
        SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<List<PrivateChatMember>>
        GetByChatIdAsync(int privateChatId)
    {
        return await _context.PrivateChatMembers
            .Where(pm => pm.PrivateChatId == privateChatId)
            .Include(pm => pm.User)
            .ToListAsync();
    }

    public async Task<PrivateChatMember?> GetMembershipAsync(
        int privateChatId,
        int userId)
    {
        return await _context.PrivateChatMembers
            .Include(pm => pm.User)
            .FirstOrDefaultAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == userId);
    }

    public async Task<bool> IsMemberAsync(
        int privateChatId,
        int userId)
    {
        return await _context.PrivateChatMembers
            .AnyAsync(pm =>
                pm.PrivateChatId == privateChatId &&
                pm.UserId == userId);
    }

    public async Task AddAsync(PrivateChatMember member)
    {
        await _context.PrivateChatMembers.AddAsync(member);
    }

    public Task DeleteAsync(PrivateChatMember member)
    {
        _context.PrivateChatMembers.Remove(member);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}