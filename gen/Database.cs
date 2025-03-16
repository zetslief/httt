using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace gen;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Topic> Topics { get; set; }
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
    public required int TopicId { get; set; }
    public required string Name { get; set; }
}