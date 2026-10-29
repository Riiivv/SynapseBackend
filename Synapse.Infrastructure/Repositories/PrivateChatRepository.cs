using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class PrivateChatRepository : IPrivateChatRepository
{
    private readonly SynapseDbContext _context;

    public PrivateChatRepository(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<PrivateChat?> GetByIdAsync(int id)
    {
        return await _context.PrivateChats
            .FirstOrDefaultAsync(pc => pc.Id == id);
    }

    public async Task<List<PrivateChat>> GetForUserAsync(int userId)
    {
        return await _context.PrivateChatMembers
            .Where(pm => pm.UserId == userId)
            .Select(pm => pm.PrivateChat!)
            .ToListAsync();
    }

    public async Task<PrivateChat?> GetBetweenUsersAsync(
        int firstUserId,
        int secondUserId)
    {
        return await _context.PrivateChats
            .Where(pc =>
                _context.PrivateChatMembers.Any(pm =>
                    pm.PrivateChatId == pc.Id &&
                    pm.UserId == firstUserId)
                &&
                _context.PrivateChatMembers.Any(pm =>
                    pm.PrivateChatId == pc.Id &&
                    pm.UserId == secondUserId))
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(PrivateChat privateChat)
    {
        await _context.PrivateChats.AddAsync(privateChat);
    }

    public Task DeleteAsync(PrivateChat privateChat)
    {
        _context.PrivateChats.Remove(privateChat);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}