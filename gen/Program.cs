using gen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

using var httpClient = new HttpClient()
{
    Timeout = TimeSpan.FromMinutes(10)
};
int counter = 0;
while (true)
{
    int topicIndex = Random.Shared.Next(topics.Length);
    var topic = topics[topicIndex];
    logger.LogInformation("{Counter} | Writing article about: {TopicName}.", counter, topic.Name);
    bool success = await GenerateArticleAsync(app.Services, httpClient, topic, logger);
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
    string topicSourceName = topic.Source.Name.Replace(".json", string.Empty);
    if (topicSourceName.Length == 0)
    {
        logger.LogError("Failed to get topic source name from {TopicSource}. Got: {TopicSourceName}", topic.Source, topicSourceName);
        return false;
    }

    string result = await GenerateTextAsync(
        httpClient,
        $"Tell an imaginary story about {topic.Name}? Topic: {topicSourceName}. Use the first sentence is a title.",
        "Story is split into paragraphs. Write at least 100 paragraphs, each consisting from 10 to 50 sentences. Don't use markdown.");

    if (result.StartsWith("Error"))
    {
        logger.LogError("Failed generation attempt. {ErrorResult}", result);
        return false;
    }

    string[] paragraphs = result.Split('\n');
    string articleTitle = $"{paragraphs[0]}";
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
