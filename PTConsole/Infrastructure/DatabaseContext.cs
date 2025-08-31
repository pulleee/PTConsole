using Microsoft.EntityFrameworkCore;
using PTConsole.Models;

namespace PTConsole.Infrastructure
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Client
            modelBuilder.Entity<Client>(
                b =>
                {
                    b.ToTable("Client");
                    b.HasKey(e => e.Id);
                });

            // Note
            modelBuilder.Entity<Note>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.HasOne(e => e.User);
                });


            // Project
            modelBuilder.Entity<Project>(
                b =>
                {
                    b.HasKey(e => e.Id);
                    b.HasOne(e => e.Client)
                        .WithMany()
                        .OnDelete(DeleteBehavior.SetNull);
                    b.HasMany(e => e.Users)
                        .WithMany(e => e.Projects);
                    b.HasMany(e => e.Tasks);
                });


            // Session
            modelBuilder.Entity<Session>(
                b =>
                {
                    b.HasOne(e => e.User)
                          .WithMany()
                          .OnDelete(DeleteBehavior.Cascade);
                    b.HasOne(e => e.Project);
                });


            // Task
            modelBuilder.Entity<WorkTask>(
                b=>
                {
                    b.HasKey(o => o.Id);
                });


            // User
            modelBuilder.Entity<User>(
                b =>
                {
                    b.HasKey(o => o.Id);
                    b.HasMany(r => r.Sessions)
                        .WithOne(r => r.User);
                    b.HasMany(r => r.Projects)
                        .WithMany(r => r.Users);
                });
        }
    }
}
