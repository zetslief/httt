using Microsoft.EntityFrameworkCore;

namespace gen;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<Topic> Topics { get; set; }
    public DbSet<TopicSource> TopicSources { get; set; }

    public DbSet<Article> Articles { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<Request> Requests { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TopicSource>()
            .HasIndex(e => e.Name)
            .IsUnique();
        modelBuilder.Entity<TopicSource>()
            .Property(e => e.Name)
            .HasMaxLength(1024 * 10);

        modelBuilder.Entity<Topic>()
            .Property(t => t.Name)
            .HasMaxLength(512);

        modelBuilder.Entity<Article>()
            .HasIndex(e => e.CreatedOn);
        modelBuilder.Entity<Article>()
            .HasIndex(e => e.ViewCount);
    }
}

public class Topic
{
    public int TopicId { get; init; }
    public required string Name { get; init; }
    public required TopicSource Source { get; init; }
}

public class TopicSource
{
    public int TopicSourceId { get; init; }
    public required string Name { get; init; }
    public required string Content { get; init; }
}

public class Article
{
    public Guid ArticleId { get; init; }
    public required string Title { get; init; }
    public required DateTime CreatedOn { get; init; }
    public int ViewCount { get; set; }

    public IEnumerable<Section>? Sections { get; init; }
    public required Topic Topic { get; init; }
}

public class Section
{
    public int SectionId { get; init; }
    public required string Content { get; init; }
    public required Article Article { get; init; }
}

public class Request
{
    public required Guid RequestId { get; init; }
    public required string Path { get; init; }
    public required DateTimeOffset RequestedOn { get; init; }
    public required string CallerIP { get; init; }
    public required int ResponseStatusCode { get; init; }
    public required string RawHeadersString { get; init; }
}