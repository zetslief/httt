using System.Text.Json;
using System.Linq;
using System.Collections.Immutable;
using gen;

ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, 1, "Data file");

var filePath = args[0];
Console.WriteLine(filePath);

if (!Path.Exists(filePath)) throw new ArgumentException($"{filePath} does not exist!");

var content = File.ReadAllText(filePath);
var items = JsonSerializer.Deserialize<ImmutableArray<string>>(content)
    .Distinct()
    .ToImmutableArray();

Console.WriteLine($"{items.Length} found.");

await using var dbContext = new DataContextFactory().CreateDbContext([]);

var topicSource = await dbContext.TopicSources.AddAsync(new()
{
    Name = Path.GetFileName(filePath),
    Content = content
});

Console.WriteLine($"Topic source added: {topicSource.Entity.Name}");

await dbContext.Topics.AddRangeAsync(items.Select(i => new Topic()
{
    Name = i,
    Source = topicSource.Entity
}));

Console.WriteLine($"Topics are written!");

await dbContext.SaveChangesAsync();

Console.WriteLine("Changes are saved!");