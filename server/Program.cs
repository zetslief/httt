using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => {
    const string fencingPath = "./articles/fencing.json";
    var article = JsonSerializer.Deserialize<Article>(File.ReadAllText(fencingPath))
        ?? CreateErrorArticle(fencingPath);
    return Results.Content(ToHtml(article), "text/html");
});

app.Run();

static string ToHtml(Article article)
{
    var builder = new StringBuilder();
    builder.AppendLine($"<div>");
    builder.AppendLine($"\t<h1>{article.Title}</h1>");
    foreach (var section in article.Sections ?? [])
    {
        builder.AppendLine($"\t\t<h2>{section.Title}</h2>");
        builder.AppendLine($"\t\t<p>{section.Content}</p>");
    }
    builder.AppendLine($"</div>");
    return builder.ToString();
}

static Article CreateErrorArticle(string articleFilePath) => new(
    $"Error while generating {articleFilePath}",
    []
);

record Article(string Title, IEnumerable<Section>? Sections);
record Section(string Title, string Content, IEnumerable<Section>? SubSection);