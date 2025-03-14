using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => {
    const string fencingPath = "./articles/fencing.json";
    return JsonSerializer.Deserialize<Article>(File.ReadAllText(fencingPath));
});

app.Run();

record Article(string Title, IEnumerable<Section>? Sections);
record Section(string Title, string Content, IEnumerable<Section>? SubSection);