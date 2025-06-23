using System.Diagnostics;
using System.Linq.Expressions;
using gen;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using server;

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
        .Select(ToArticleLink())
        .ToArrayAsync();
    var topViewedArticles = await ctx.Articles
        .OrderByDescending(a => a.ViewCount)
        .Take(500)
        .Select(ToArticleLink())
        .ToArrayAsync();
    var leastViewedArticles = await ctx.Articles
        .OrderBy(a => a.ViewCount)
        .Take(500)
        .Select(ToArticleLink())
        .ToArrayAsync();
    var htmlBuilder = new HtmlBuilder()
        .AddMainHeader()
        .AddFlexBox([
            itemBuilder => itemBuilder.AddArticleList("New", 0, newestArticles.Select(TruncateArticleTitle)),
            itemBuilder => itemBuilder.AddArticleList("Top viewed", 0, topViewedArticles.Select(TruncateArticleTitle)),
            itemBuilder => itemBuilder.AddArticleList("Least viewed", 0, leastViewedArticles.Select(TruncateArticleTitle)),
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

app.MapGet("/articles/", (DataContext ctx, [FromQuery] int startIndex, [FromQuery] int length) => GetArticles(ctx, startIndex, length));
// This endpoint is here just for backwards compatibility, in case crawlers would want to use it.
app.MapGet("/articles/{startIndex}/{length}", GetArticles);

static async Task<IResult> GetArticles(DataContext ctx, int startIndex, int length)
{
    // TODO: change that to validation errors.
    if (length <= 0 || startIndex < 0) return Results.BadRequest($"Invalid path parameters: {length}, {startIndex}.");
    if (length > 10000) return Results.BadRequest($"Length is greater than maximum. Length is {length}. Maximum is 10000.");
    var totalCount = await ctx.Articles.CountAsync();
    if (totalCount <= startIndex) return Results.BadRequest($"There are {totalCount} of articles. Cannot show {length} articles at {startIndex}");
    var articles = await ctx.Articles.OrderByDescending(a => a.CreatedOn)
        .Skip(startIndex)
        .Take(length)
        .Select(ToArticleLink())
        .ToArrayAsync();
    var ranges = Enumerable.Range(0, totalCount)
        .Chunk(length)
        .Select(range => new ArticleRange(range.First(), length))
        .ToList();
    var htmlBuilder = new HtmlBuilder()
        .AddGoHomeHeader()
        .AddRanges(ranges)
        .AddArticleList(
            $"Articles: {startIndex} - {(startIndex + articles.Length)}",
            startIndex,
            articles.Select(TruncateArticleTitle));
    return Results.Content(htmlBuilder.Build(), "text/html");
}

app.Use(async (httpContext, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    var callerIp = httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedIp)
            ? forwardedIp
            : (httpContext.Request.Headers.TryGetValue("Cf-Connecting-IP", out var cfConnectionIp)
                ? cfConnectionIp
                : default);

    var beforeNextMiddleware = stopwatch.Elapsed;

    await next(httpContext);

    var afterNextMiddleware = stopwatch.Elapsed;

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
    var logger = httpContext.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation(
        "{EntityResponseStatusCode} > {EntityPath} | {EntityCallerIp} | Total duration: {StopwatchElapsed} | Middleware: {BeforeNextMiddleware} | Log: {AfterNextMiddleware}",
        request.Entity.ResponseStatusCode,
        request.Entity.Path,
        request.Entity.CallerIP,
        stopwatch.Elapsed.TotalMilliseconds,
        (afterNextMiddleware - beforeNextMiddleware).TotalMilliseconds,
        (stopwatch.Elapsed - afterNextMiddleware).TotalMilliseconds);
});

app.Run();

const int titleMaxLength = 200;

static Expression<Func<gen.Article, ArticleLink>> ToArticleLink() => article => new(
    article.ArticleId,
    article.Title,
    article.CreatedOn,
    article.ViewCount
);

static ArticleLink TruncateArticleTitle(ArticleLink articleLink) => articleLink with
{
    Title = articleLink.Title.Length > titleMaxLength
        ? $"{articleLink.Title[..(titleMaxLength - 3)]}..."
        : articleLink.Title
};

public record ArticleLink(Guid Id, string Title, DateTime CreatedOn, int ViewCount);
public record Article(string Title, IEnumerable<Section>? Sections);
public record Section(string Title, string Content, IEnumerable<Section>? SubSection);
public record ArticleRange(int Start, int Length);