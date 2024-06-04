using Microsoft.EntityFrameworkCore;
using Scribe.Data.Model;

namespace Scribe.Data.Database;

public class ScribeContext : DbContext
{
    public DbSet<Folder> Folders { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(@"Data Source=.\scribe_database.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Document>(d => d.HasKey("_id"))
            .Entity<Tag>(t => t.HasKey("_id"));

        modelBuilder.Entity<Folder>()
            .HasMany<Document>(f => f.Documents)
            .WithOne()
            .HasForeignKey(e => e.FolderId)
            .IsRequired();
            
        modelBuilder.Entity<Folder>().Navigation(e => e.Documents).AutoInclude();
    }
}