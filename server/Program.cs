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
    var articles = await ctx.Articles
        .OrderByDescending(a => a.CreatedOn)
        .Select(a => new ArticleLink(a.ArticleId, a.Title, a.CreatedOn, a.ViewCount))
        .ToArrayAsync();
    return Results.Content(ToArticleListHtml(articles), "text/html");
});

app.MapGet("/article/{articleId}", async (DataContext ctx, Guid articleId) => {
    await ctx.Articles.Where(a => a.ArticleId == articleId)
        .ExecuteUpdateAsync(s => s.SetProperty(a => a.ViewCount, a => a.ViewCount + 1));
    var article = await ctx.Articles.Include(article => article.Sections).SingleAsync(a => a.ArticleId == articleId);
    return Results.Content(ArticleToHtml(new(
        article.Title,
        article.Sections?.Select(s => new Section(string.Empty, s.Content, null)).ToArray() ?? []
    )), "text/html");
});

app.Run();

static string ToArticleListHtml(IReadOnlyCollection<ArticleLink> articles) => new HtmlBuilder()
    .WithTag("div", builder => builder
        .AddHeader(1, $"There are {articles.Count} articles:")
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
/*
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
*/

static Article CreateErrorArticle(string articleFilePath) => new(
    $"Error while generating {articleFilePath}",
    []
);

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

    public HtmlBuilder AddTag(string tag, string content)
    {
        AppendLine($"<{tag}>{content}</{tag}>");
        return this;
    }

    public HtmlBuilder WithTag(string tag, Action<HtmlBuilder> buildInner)
    {
        AppendLine($"<{tag}>");
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
        this.builder.AppendLine(content);
    }

    public string Build()
    {
        builder.AppendLine($"\t</body>");
        builder.AppendLine($"\t</html>");
        return builder.ToString();
    }
}