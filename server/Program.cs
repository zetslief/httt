using System.Text;
using System.Text.Json;
using gen;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddDbContext<DataContext>(options =>
{
    var dataFile = $"{builder.Configuration.GetRequiredSection("dataFolder").Value}/secret_data.db";
    options.UseSqlite($"Data Source={dataFile}");
});

var app = builder.Build();

app.MapGet("/", async (DataContext ctx) => {
    var articles = await ctx.Articles.Select(a => new ArticleLink(a.ArticleId, a.Title)).ToArrayAsync();
    return Results.Content(ToArticleListHtml(articles), "text/html");
});
app.MapGet("/article/{articleId}", async (DataContext ctx, Guid articleId) => {
    var article = await ctx.Articles.Include(article => article.Sections).SingleAsync(a => a.ArticleId == articleId);
    return Results.Content(ArticleToHtml(new(
        article.Title,
        article.Sections?.Select(s => new Section(string.Empty, s.Content, null)).ToArray() ?? []
    )), "text/html");
});

app.Run();

static string ToArticleListHtml(IEnumerable<ArticleLink> articles)
{
    var builder = new StringBuilder();
    builder.AppendLine("<div>");
    builder.AppendLine($"\t<h1>Articles</h1>");
    builder.AppendLine("<ol>");
    foreach (var article in articles)
    {
        builder.AppendLine($"\t<li><a href='/article/{article.Id}'>{article.Title}</a></li>");
    }
    builder.AppendLine("</ol");
    builder.AppendLine("</div>");
    return builder.ToString();
}

static string ArticleToHtml(Article article)
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

record ArticleLink(Guid Id, string Title);
record Article(string Title, IEnumerable<Section>? Sections);
record Section(string Title, string Content, IEnumerable<Section>? SubSection);

class HtmlBuilder()
{
    private readonly StringBuilder builder = new(1024);

    public HtmlBuilder AddHeader(int number, string content)
    {
        builder.AppendLine($"\t<h{number}>{content}</h{number}>");
        return this;
    }

    public HtmlBuilder AddP(string content)
    {
        builder.AppendLine($"<p>{content}</p>");
        return this;
    }

    public string Build()
    {
        builder.AppendLine($"\t</body>");
        builder.AppendLine($"\t</html>");
        return builder.ToString();
    }
}