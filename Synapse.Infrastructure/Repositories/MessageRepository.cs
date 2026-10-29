using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Interfaces;
using Synapse.Domain.Models;
using Synapse.Infrastructure.Data;

namespace Synapse.Infrastructure.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly SynapseDbContext _context;

    public MessageRepository(SynapseDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        return await _context.Messages
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<Message>> GetChannelMessagesAsync(int channelId)
    {
        return await _context.Messages
            .Where(m => m.ChannelId == channelId)
            .Include(m => m.User)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task<List<Message>> GetPrivateMessagesAsync(
        int privateChatId)
    {
        return await _context.Messages
            .Where(m => m.PrivateChatId == privateChatId)
            .Include(m => m.User)
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
    }

    public Task UpdateAsync(Message message)
    {
        _context.Messages.Update(message);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Message message)
    {
        _context.Messages.Remove(message);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}