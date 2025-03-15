using System.Diagnostics;
using Microsoft.Extensions.Configuration;

using static Gemini.Gemini;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var dataFolder = configuration.GetRequiredSection("dataFolder").Value;
Debug.Assert(dataFolder is not null);
Console.WriteLine(Path.GetFullPath(dataFolder));

using var client = new HttpClient();

Console.WriteLine(await GenerateTextAsync(client, "Select random sport"));