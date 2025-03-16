using System.Diagnostics;
using System.Text.Json;
using gen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using static Gemini.Gemini;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .Build();

await using var dbContext = new DataContextFactory().CreateDbContext(args);
var ts = await dbContext.Topics.Select(t => t).CountAsync();
Console.WriteLine($"There are {ts} topics in the database.");

Environment.Exit(11);

var dataFolder = configuration.GetRequiredSection("dataFolder").Value;
Debug.Assert(dataFolder is not null);
var fullPath = Path.GetFullPath(dataFolder);
Debug.Assert(fullPath is not null);
var data = Directory.GetFiles(fullPath, "*.json").First();
var topics = JsonSerializer.Deserialize<IEnumerable<string>>(File.ReadAllText(data))
    ?? throw new InvalidOperationException("Failed to deserialize topics");
using var client = new HttpClient();
foreach (var topic in topics)
{
    Console.WriteLine($"========== {topic} ==========");
    Console.WriteLine(await GenerateTextAsync(client, $"Tell me a little bit about {topic}"));
    Console.WriteLine();
}

