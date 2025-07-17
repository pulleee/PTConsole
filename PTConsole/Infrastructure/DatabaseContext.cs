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
            var clientBuilder = modelBuilder.Entity<Client>().ToTable("Client");
            clientBuilder.HasKey(o => o.Id);

            // Note
            var noteBuilder = modelBuilder.Entity<Note>().ToTable("Note");
            noteBuilder.HasKey(o => o.Id);
            noteBuilder.HasOne(r => r.User);

            // Project
            var projectBuilder = modelBuilder.Entity<Project>().ToTable("Project");
            projectBuilder
                .HasKey(o => o.Id);
            projectBuilder
                .HasOne(r => r.Client)
                .WithMany()
                .OnDelete(DeleteBehavior.SetNull);
            projectBuilder
                .HasMany(r => r.Users)
                .WithMany(r => r.Projects);
            projectBuilder
                .HasMany(r => r.Tasks);

            // Session
            var sessionBuilder = modelBuilder.Entity<Session>().ToTable("Session");
            sessionBuilder
                .HasOne(r => r.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Cascade);
            sessionBuilder
                .HasOne(r => r.Project);

            // Task
            var workTaskBuilder = modelBuilder.Entity<WorkTask>().ToTable("Task");
            workTaskBuilder.HasKey(o => o.Id);

            // User
            var userBuilder = modelBuilder.Entity<User>().ToTable("User");
            userBuilder.HasKey(o => o.Id);
            userBuilder.HasMany(r => r.Sessions)
                .WithOne(r => r.User);
            userBuilder.HasMany(r => r.Projects)
                .WithMany(r => r.Users);
        }
    }
}
