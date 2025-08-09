using gen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using static Gemini.Gemini;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.Database)
    .ValidateDataAnnotations();

builder.Services.AddDbContext<DataContext>(DataContextHelpers.Configure);

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();

Topic[] topics = [];

await RunWithDatabaseAsync(app.Services, async ctx =>
{
    topics = await ctx.Topics.Include(t => t.Source).ToArrayAsync();
});

logger.LogInformation("There are {TopicsLength} known topics.", topics.Length);

if (topics.Length == 0) Environment.Exit(1);

using var httpClient = new HttpClient();
int counter = 0;
while (true)
{
    var topicIndex = Random.Shared.Next(topics.Length);
    var topic = topics[topicIndex];
    logger.LogInformation("{Counter} | Writing article about: {TopicName}.", counter, topic.Name);
    var success = await GenerateArticleAsync(app.Services, httpClient, topic, logger);
    var delay = success ? TimeSpan.FromSeconds(1) : TimeSpan.FromMinutes(1);
    logger.LogInformation("{I} | {Success}! Waiting: {Delay}", counter++, success ? "Success" : "Error", delay);
    await Task.Delay(delay);
}

static async Task RunWithDatabaseAsync(IServiceProvider serviceProvider, Func<DataContext, Task> action)
{
    await using var scope = serviceProvider.CreateAsyncScope();
    var ctx = scope.ServiceProvider.GetRequiredService<DataContext>();
    await action(ctx);
}

static async Task<bool> GenerateArticleAsync(IServiceProvider serviceProvider, HttpClient httpClient, Topic topic, ILogger logger)
{
    var topicSourceName = topic.Source.Name.Replace(".json", string.Empty);
    if (topicSourceName.Length == 0)
    {
        logger.LogError("Failed to get topic source name from {TopicSource}. Got: {TopicSourceNamme}", topic.Source, topicSourceName);
        return false;
    }

    var result = await GenerateTextAsync(
        httpClient,
        $"What's new about {topic.Name} from {topic.Source.Name}?",
        "At least 20000 tokens. Don't use markdown. First sentence is a title maximum 200 symbols long. Story is split into paragraphs.");

    if (result.StartsWith("Error"))
    {
        logger.LogError("Failed generation attempt. {ErrorResult}", result);
        return false;
    }

    var paragraphs = result.Split('\n');
    var articleTitle = $"{paragraphs[0]}";
    await RunWithDatabaseAsync(serviceProvider, async ctx =>
    {
        var article = ctx.Articles.Add(new()
        {
            ArticleId = Guid.NewGuid(),
            Title = articleTitle,
            Topic = topic,
            CreatedOn = DateTime.UtcNow
        });
        ctx.Attach(topic);
        ctx.Attach(topic.Source);
        await ctx.Sections.AddRangeAsync(paragraphs
            .Skip(1)
            .Where(p => p.Length > 5 && !string.IsNullOrWhiteSpace(p))
            .Select(p => new Section { Content = p, Article = article.Entity }));
        await ctx.SaveChangesAsync();
    });
    return true;
}