using Microsoft.EntityFrameworkCore;
using nodiCore.Models;

namespace nodiCore.Data;

/// <summary>
/// EF Core database context for the application. Supports SQLite and PostgreSQL;
/// the provider is selected at startup in <c>Program.cs</c>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NoteTag uses a composite PK instead of a surrogate key to naturally
        // prevent duplicate tag assignments on the same note.
        modelBuilder.Entity<NoteTag>()
            .HasKey(nt => new { nt.NoteId, nt.TagId });

        modelBuilder.Entity<NoteTag>()
            .HasOne(nt => nt.Note)
            .WithMany(n => n.NoteTags)
            .HasForeignKey(nt => nt.NoteId);

        modelBuilder.Entity<NoteTag>()
            .HasOne(nt => nt.Tag)
            .WithMany(t => t.NoteTags)
            .HasForeignKey(nt => nt.TagId);

        // Unique indexes so duplicate usernames/emails are rejected at the DB level,
        // even if the service-layer checks are bypassed by concurrent requests.
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username).IsUnique();
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<AppSetting>()
            .HasIndex(s => s.Key).IsUnique();
    }
}
