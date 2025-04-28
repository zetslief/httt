using gen;
using Microsoft.EntityFrameworkCore;
using static Gemini.Gemini;

Topic[] topics = [];
await using (var ctx = new DataContextFactory().CreateDbContext(args))
{
    topics = await ctx.Topics.Include(t => t.Source).ToArrayAsync();
}

Console.WriteLine($"There are {topics.Length} known topics.");

if (topics.Length == 0) Environment.Exit(1);

using var httpClient = new HttpClient();
int counter = 0;
while (true)
{
    var topicIndex = Random.Shared.Next(topics.Length);
    var topic = topics[topicIndex];
    Console.WriteLine($"{counter} | Writing article about: {topic.Name}.");
    var success = await GenerateArticleAsync(httpClient, topic);
    var delay = success ? TimeSpan.FromSeconds(1) : TimeSpan.FromMinutes(1);
    Console.WriteLine($"{counter++} | {(success ? "Success" : "Error")}! Waiting: {delay}");
    await Task.Delay(delay);
}

static async Task<bool> GenerateArticleAsync(HttpClient httpClient, Topic topic)
{
    var result = await GenerateTextAsync(
        httpClient,
        $"What's new about {topic.Name} from {topic.Source.Name}?",
        "At least 20000 tokens, don't use markdown, first sentence is a title");

    if (result.StartsWith("Error")) return false;

    var paragraphs = result.Split('\n');
    var articleTitle = $"{paragraphs[0]}";
    await using var ctx = new DataContextFactory().CreateDbContext([]);
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
    return true;
}