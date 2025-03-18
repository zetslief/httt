using System.Diagnostics;
using System.Text.Json;
using gen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using static Gemini.Gemini;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .Build();

await using var ctx = new DataContextFactory().CreateDbContext(args);

var count = await ctx.Topics.CountAsync();
Console.WriteLine($"there are {count} known topics.");

foreach (var source in ctx.TopicSources)
{
    Console.WriteLine($"Topic: {source.Name} has content length of {source.Content.Length}");
}

using var httpClient = new HttpClient();
foreach (var topic in ctx.Topics)
{
    var result = await GenerateTextAsync(
        httpClient,
        $"What's new about {topic.Name}?",
        "At least 20000 tokens, don't use markdown, first sentence is a title");
    if (result.StartsWith("Error")) continue;
    var paragraphs = result.Split('\n');
    var articleTitle = $"{paragraphs[0]}";
    var article = ctx.Articles.Add(new() { Title = articleTitle, Topic = topic, CreatedOn = DateTime.UtcNow });
    await ctx.Sections.AddRangeAsync(paragraphs
        .Skip(1)
        .Where(p => p.Length > 5 && !string.IsNullOrWhiteSpace(p))
        .Select(p => new Section { Content = p, Article = article.Entity}));
    await ctx.SaveChangesAsync();
    Console.WriteLine($"{article.Entity.Title} is written into the database.");
    await Task.Delay(TimeSpan.FromSeconds(1));
}

static async Task ExploreDataSource(DataContext ctx, IConfiguration configuration)
{
    var dataFolder = configuration.GetRequiredSection("dataFolder").Value;
    Debug.Assert(dataFolder is not null);
    var fullPath = Path.GetFullPath(dataFolder);
    Debug.Assert(fullPath is not null);
    var dataFiles = Directory.GetFiles(fullPath, "*.json");
    foreach (var dataFile in dataFiles)
    {
        await WriteTopicFileToDatabaseAsync(ctx, dataFile);
    }
}

static async Task<string> WriteTopicFileToDatabaseAsync(DataContext ctx, string file)
{
    var fullFileName = Path.GetFullPath(file);
    if (!Path.Exists(fullFileName)) return $"{fullFileName} does not exist";
    var fileContent = File.ReadAllText(fullFileName);
    var topics = JsonSerializer.Deserialize<IEnumerable<string>>(fileContent);
    if (topics is null) return $"Failed to deserialize file content.\n"
        + $"File Path: {fullFileName}\n"
        + $"Content {fileContent}";
    var source = await ctx.TopicSources.AddAsync(new() { Name = Path.GetFileName(fullFileName), Content = fileContent }); 
    await ctx.Topics
        .AddRangeAsync(topics.Select(topic => new Topic() { Name = topic, Source = source.Entity }));
    await ctx.SaveChangesAsync();
    return string.Empty;
}
