using System.Diagnostics;
using System.Text;
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

app.UseStaticFiles();

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
    var leastViewedArticles = await ctx.Articles
        .OrderBy(a => a.ViewCount)
        .Take(500)
        .Select(a => new ArticleLink(a.ArticleId, a.Title, a.CreatedOn, a.ViewCount))
        .ToArrayAsync();
    var htmlBuilder = new HtmlBuilder()
        .AddMainHeader()
        .AddFlexBox([
            ToArticleListHtml("New", newestArticles),
            ToArticleListHtml("Top viewed", topViewedArticles),
            ToArticleListHtml("Least viewed", leastViewedArticles),
        ]);
    return Results.Content(htmlBuilder.Build(), "text/html");
});

app.MapGet("/article/{articleId}", async (DataContext ctx, Guid articleId) =>
{
    await ctx.Articles.Where(a => a.ArticleId == articleId)
        .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));
    var article = await ctx.Articles.Include(article => article.Sections).SingleAsync(a => a.ArticleId == articleId);
    var htmlBuilder = new HtmlBuilder()
        .AddGoHomeHeader()
        .AddArticle(new(
            article.Title,
            article.Sections?.Select(s => new Section(string.Empty, s.Content, null)).ToArray() ?? [])
        );
    return Results.Content(htmlBuilder.Build(), "text/html");
});

app.MapGet("/articles/{startIndex}/{length}", async (DataContext ctx, int startIndex, int length) =>
{
    // TODO: change that to validation errors.
    if (length <= 0 || startIndex < 0) return Results.BadRequest($"Invalid path parameters: {length}, {startIndex}.");
    var totalCount = await ctx.Articles.CountAsync();
    if (totalCount <= startIndex) return Results.BadRequest($"There are {totalCount} of articles. Cannot show {length} articles at {startIndex}");
    var articles = await ctx.Articles.OrderByDescending(a => a.CreatedOn)
        .Skip(startIndex)
        .Take(length)
        .Select(a => new ArticleLink(a.ArticleId, a.Title, a.CreatedOn, a.ViewCount))
        .ToArrayAsync();
    return Results.Content(ToArticleListHtml($"Articles: {startIndex} - {(startIndex + articles.Length)}", articles), "text/html");
});

app.Use(async (httpContext, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    var callerIp = httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIp)
            ? forwardedIp
            : (httpContext.Request.Headers.TryGetValue("Cf-Connecting-IP", out var cfConnectionIp)
                ? cfConnectionIp
                : default);

    await next(httpContext);

    var dataContext = httpContext.RequestServices.GetRequiredService<DataContext>();
    var request = await dataContext.Requests.AddAsync(new()
    {
        RequestId = Guid.NewGuid(),
        Path = httpContext.Request.Path,
        RequestedOn = DateTimeOffset.UtcNow,
        ResponseStatusCode = httpContext.Response.StatusCode,
        CallerIP = callerIp.ToString(),
        RawHeadersString = string.Join(',', httpContext.Request.Headers)
    });
    await dataContext.SaveChangesAsync();
    stopwatch.Stop();
    Console.WriteLine($"{request.Entity.ResponseStatusCode} > {request.Entity.Path} | {request.Entity.CallerIP} | Request duration: {stopwatch.Elapsed}");
});

app.Run();

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

static class HtmlBuilderExtensions
{
    public static HtmlBuilder AddMainHeader(this HtmlBuilder builder) =>
        builder.WithTag("div", static builder =>
        {
            builder.AddHeader(1, "Your Daily Slop");
            builder.AddA("./articles/1/1000", "All articles");
        });

    public static HtmlBuilder AddGoHomeHeader(this HtmlBuilder builder) =>
        builder.WithTag("div", static builder =>
        {
            builder.AddHeader(1, "Your Daily Slop");
            builder.AddA("/", "Home");
        });

    public static HtmlBuilder AddFlexBox(this HtmlBuilder flexBoxBuilder, IEnumerable<string> children) => flexBoxBuilder
        .WithTag("div", builder =>
        {
            foreach (var child in children) builder.AddTag("div", child, style: "flex: 1 1 0;");
        }, style: "display: flex;");

    public static HtmlBuilder AddArticle(this HtmlBuilder articleBuilder, Article article) => articleBuilder
        .WithTag("div", builder =>
        {
            builder.AddHeader(1, article.Title);
            foreach (var section in article.Sections ?? [])
            {
                builder
                    .AddHeader(2, section.Title)
                    .AddTag("p", section.Content);
            }
        });
}

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