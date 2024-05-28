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
}