using System.Collections.Immutable;
using System.Text.Json;
using gen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

ArgumentOutOfRangeException.ThrowIfLessThan(args.Length, 1, "Data file");

var builder = Host.CreateApplicationBuilder();

builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.Database)
    .ValidateDataAnnotations();

builder.Services.AddDbContext<DataContext>(DataContextHelpers.Configure);

var app = builder.Build();

var filePath = args[0];
Console.WriteLine(filePath);

if (!Path.Exists(filePath)) throw new ArgumentException($"{filePath} does not exist!");

var content = File.ReadAllText(filePath);
var items = JsonSerializer.Deserialize<ImmutableArray<string>>(content)
    .Distinct()
    .ToImmutableArray();

Console.WriteLine($"{items.Length} found.");

await using var scope = app.Services.CreateAsyncScope();
await using var dbContext = scope.ServiceProvider.GetRequiredService<DataContext>();

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
