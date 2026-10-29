using Microsoft.EntityFrameworkCore;
using Synapse.Domain.Models;

namespace Synapse.Infrastructure.Data;

public class SynapseDbContext : DbContext
{
    public SynapseDbContext(DbContextOptions<SynapseDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Server> Servers { get; set; }

    public DbSet<ServerMember> ServerMembers { get; set; }

    public DbSet<Channel> Channels { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<PrivateChat> PrivateChats { get; set; }

    public DbSet<PrivateChatMember> PrivateChatMembers { get; set; }
    public DbSet<ServerInvite> ServerInvites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<ServerMember>()
            .HasIndex(sm => new { sm.ServerId, sm.UserId })
            .IsUnique();

        modelBuilder.Entity<PrivateChatMember>()
            .HasIndex(pm => new { pm.PrivateChatId, pm.UserId })
            .IsUnique();

        modelBuilder.Entity<ServerInvite>()
    .HasIndex(i => i.Code)
    .IsUnique();
    }
}