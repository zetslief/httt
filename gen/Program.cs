using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

using static Gemini.Gemini;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appSettings.json")
    .Build();

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

