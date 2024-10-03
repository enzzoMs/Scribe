using Microsoft.EntityFrameworkCore;
using Scribe.Data.Model;

namespace Scribe.Data.Database;

public class ScribeContext : DbContext
{
    public DbSet<Folder> Folders { get; private set; }
    public DbSet<Document> Documents { get; private set; }
    public DbSet<Tag> Tags { get; private set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(@"Data Source=./Database/scribe_database.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Folder>()
            .HasMany<Document>(f => f.Documents)
            .WithOne()
            .HasForeignKey(d => d.FolderId)
            .IsRequired();

        modelBuilder.Entity<Folder>()
            .HasMany<Tag>(f => f.Tags)
            .WithOne()
            .HasForeignKey(t => t.FolderId);

        modelBuilder.Entity<Document>()
            .HasMany<Tag>(f => f.Tags)
            .WithMany();

        modelBuilder.Entity<Tag>()
            .HasKey(t => new { t.Name, t.FolderId });
        
        modelBuilder.Entity<Folder>().Navigation(e => e.Documents).AutoInclude();
        modelBuilder.Entity<Folder>().Navigation(e => e.Tags).AutoInclude();
        modelBuilder.Entity<Document>().Navigation(e => e.Tags).AutoInclude();
    }
}