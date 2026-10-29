using Synapse.Domain.Models;

namespace Synapse.Domain.Interfaces;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();

    Task<User?> GetByIdAsync(int id);

    Task<User?> GetByUsernameAsync(string username);

    Task<User?> GetByEmailAsync(string email);

    Task<bool> UsernameExistsAsync(string username);

    Task<bool> EmailExistsAsync(string email);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task DeleteAsync(User user);

    Task SaveChangesAsync();
}