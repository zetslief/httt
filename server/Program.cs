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

app.MapGet("/", async (DataContext ctx) =>
{
    var newestArticles = await ctx.Articles
        .OrderByDescending(a => a.CreatedOn)
        .Take(500)
        .Select(a => new ArticleLink(a.ArticleId, a.Title, a.CreatedOn, a.ViewCount))
        .ToArrayAsync();
    var topViewedArticles = await ctx.Articles
        .OrderByDescending(a => a.ViewCount)
        .Take(500)
        .Select(a => new ArticleLink(a.ArticleId, a.Title, a.CreatedOn, a.ViewCount))
        .ToArrayAsync();
    var html = ToFlexBox([
        ToArticleListHtml("New", newestArticles),
        ToArticleListHtml("Top viewed", topViewedArticles),
    ]);
    return Results.Content(html, "text/html");
});

app.MapGet("/article/{articleId}", async (DataContext ctx, Guid articleId) =>
{
    await ctx.Articles.Where(a => a.ArticleId == articleId)
        .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));
    var article = await ctx.Articles.Include(article => article.Sections).SingleAsync(a => a.ArticleId == articleId);
    return Results.Content(ArticleToHtml(new(
        article.Title,
        article.Sections?.Select(s => new Section(string.Empty, s.Content, null)).ToArray() ?? []
    )), "text/html");
});

app.Use(async (httpContext, next) =>
{
    Console.WriteLine(httpContext.Connection.Id);
    Console.WriteLine(httpContext.Connection.LocalIpAddress);
    Console.WriteLine(httpContext.Connection.LocalPort);
    Console.WriteLine(httpContext.Connection.RemoteIpAddress);
    Console.WriteLine(httpContext.Connection.RemotePort);
    Console.WriteLine(httpContext.Connection.ClientCertificate);
    Console.WriteLine(httpContext.TraceIdentifier);
    Console.WriteLine(httpContext.Request.Host);
    Console.WriteLine(httpContext.Request.IsHttps);
    Console.WriteLine(httpContext.Request.Path);
    Console.WriteLine(string.Join(',', httpContext.Request.Headers.Select(s => s.ToString()).ToArray()));
    await next(httpContext);
});

app.Run();

static string ToFlexBox(IEnumerable<string> children) => new HtmlBuilder()
    .WithTag("div", builder =>
    {
        foreach (var child in children) builder.AddTag("div", child, style: "flex: 1 1 0;");
    }, style: "display: flex;;")
    .Build();

static string ToArticleListHtml(string title, IReadOnlyCollection<ArticleLink> articles) => new HtmlBuilder()
    .WithTag("div", builder => builder
        .AddHeader(1, title)
        .WithTag("ol", listBuilder =>
        {
            foreach (var article in articles)
            {
                listBuilder.WithTag("li", listItemBuilder => listItemBuilder
                        .AddA($"/article/{article.Id}", article.Title)
                        .AddTag("p", $"{article.CreatedOn} Views: {article.ViewCount}")
                );
            }
        })
    ).Build();

static string ArticleToHtml(Article article) => new HtmlBuilder()
    .WithTag("div", builder =>
    {
        builder.AddHeader(1, article.Title);
        foreach (var section in article.Sections ?? [])
        {
            builder
                .AddHeader(2, section.Title)
                .AddTag("p", section.Content);
        }
    }).Build();

record ArticleLink(Guid Id, string Title, DateTime CreatedOn, int ViewCount);
record Article(string Title, IEnumerable<Section>? Sections);
record Section(string Title, string Content, IEnumerable<Section>? SubSection);

sealed class HtmlBuilder
{
    private readonly StringBuilder builder = new(1024);
    private int indent = 0;

    public HtmlBuilder AddHeader(int number, string content)
    {
        AppendLine($"\t<h{number}>{content}</h{number}>");
        return this;
    }

    public HtmlBuilder AddA(string href, string content)
    {
        AppendLine($"<a href='{href}'>{content}</a>");
        return this;
    }

    public HtmlBuilder AddTag(string tag, string content, string? style = null)
    {
        AppendLine($"<{tag} style='{style ?? string.Empty}'>{content}</{tag}>");
        return this;
    }

    public HtmlBuilder WithTag(string tag, Action<HtmlBuilder> buildInner, string? style = null)
    {
        AppendLine($"<{tag} style='{style ?? string.Empty}'>");
        indent += 1;
        buildInner(this);
        indent -= 1;
        AppendLine($"</{tag}>");
        return this;
    }

    private void AppendLine(string content)
    {
        for (int i = 0; i < indent; ++i)
            builder.Append('\t');
        builder.AppendLine(content);
    }

    public string Build() => builder.ToString();
}