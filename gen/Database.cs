using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace gen;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Topic> Topics { get; set; }
    public DbSet<TopicSource> TopicSources { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TopicSource>()
            .HasKey(e => new { e.TopicSourceId, e.Name });
        modelBuilder.Entity<TopicSource>()
            .Property(e => e.Name)
            .HasMaxLength(1024 * 10);
            
        modelBuilder.Entity<Topic>()
            .HasKey(table => new { table.TopicId, table.Name });
        modelBuilder.Entity<Topic>()
            .Property(t => t.Name)
            .HasMaxLength(512);
    }
}

public class DataContextFactory : IDesignTimeDbContextFactory<DataContext>
{
    public DataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataContext>();
        var connectionString = "Data Source=./../../secret_data/secret_data.db";
        optionsBuilder.UseSqlite(connectionString);
        return new(optionsBuilder.Options);
    }
}

public class Topic
{
    public int TopicId { get; set; }
    public required string Name { get; set; }
    public required TopicSource Source { get; set; }
}

public class TopicSource
{
    public int TopicSourceId { get; set; }
    public required string Name { get; set; }
    public required string Content { get; set; }
}