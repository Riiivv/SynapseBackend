using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IServerMemberRepository
{
    Task<List<ServerMember>> GetByServerIdAsync(int serverId);

    Task<ServerMember?> GetMembershipAsync(int serverId,int userId);

    Task<bool> IsMemberAsync(int serverId,int userId);

    Task AddAsync(ServerMember member);

    Task UpdateAsync(ServerMember member);

    Task DeleteAsync(ServerMember member);

    Task SaveChangesAsync();
}