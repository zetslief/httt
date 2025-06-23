namespace server;

public static class HtmlBuilderExtensions
{
    public static HtmlBuilder AddMainHeader(this HtmlBuilder builder) =>
        builder.WithTag("div", static builder => builder
            .AddHeader(1, "Your Daily Slop")
            .AddA("./articles?startIndex=0&length=1000", "All articles")
        );

    public static HtmlBuilder AddGoHomeHeader(this HtmlBuilder builder) =>
        builder.WithTag("div", static builder => builder
            .AddHeader(1, "Your Daily Slop")
            .AddA("/", "Home")
        );

    public static HtmlBuilder AddRanges(this HtmlBuilder rangesBuilder, IEnumerable<ArticleRange> ranges) => rangesBuilder
        .WithTag("div", builder =>
        {
            builder.AddHeader(2, "More articles:");
            foreach (var range in ranges)
                builder.AddA($"/articles?startIndex={range.Start}&length={range.Length}", $"{range.Start}-{(range.Start + range.Length)}");
        });

    public static HtmlBuilder AddFlexBox(this HtmlBuilder flexBoxBuilder, IEnumerable<Action<HtmlBuilder>> itemBuilders) => flexBoxBuilder
        .WithTag("div", builder =>
        {
            foreach (var itemBuilder in itemBuilders) builder.WithTag("div", itemBuilder, style: "flex: 1 1 0;");
        }, style: "display: flex;");

    public static HtmlBuilder AddArticle(this HtmlBuilder articleBuilder, Article article) => articleBuilder
        .WithTag("div", builder =>
        {
            builder.AddHeader(1, article.Title);
            foreach (var section in article.Sections ?? [])
                builder
                    .AddHeader(2, section.Title)
                    .AddTag("p", section.Content);
        });

    public static HtmlBuilder AddArticleList(this HtmlBuilder articleListBuilder, string title, int startIndex, IEnumerable<ArticleLink> articles) => articleListBuilder
        .WithTag("div", builder => builder
            .AddHeader(1, title)
            .WithTag("ol", listBuilder =>
                {
                    foreach (var article in articles)
                        listBuilder.WithTag("li", listItemBuilder => listItemBuilder
                            .AddA($"/article/{article.Id}", article.Title)
                            .AddTag("p", $"{article.CreatedOn} Views: {article.ViewCount}")
                        );
                },
                style: "list-style-position: inside;",
                attributes: $"start='{startIndex}'")
        );
}