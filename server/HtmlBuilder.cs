using System.Text;

namespace server;

public sealed class HtmlBuilder
{
    private readonly StringBuilder _builder = new(1024);
    private int _indent = 0;

    private HtmlBuilder() { }

    public HtmlBuilder AddHeader(int number, string content)
    {
        _builder.AppendIndentedLine(_indent, $"<h{number}>{content}</h{number}>");
        return this;
    }

    public HtmlBuilder AddA(string href, string content)
    {
        _builder.AppendIndentedLine(_indent, $"<a href='{href}'>{content}</a>");
        return this;
    }

    public HtmlBuilder AddTag(string tag, string content, string? style = null)
    {
        _builder.AppendIndentedLine(_indent, $"<{tag} style='{style ?? string.Empty}'>{content}</{tag}>");
        return this;
    }

    public HtmlBuilder WithTag(string tag, Action<HtmlBuilder> buildInner, string? style = null, string? attributes = null)
    {
        _builder.AppendIndentedLine(_indent, $"<{tag} style='{style ?? string.Empty}' {attributes ?? string.Empty}>");
        _indent += 1;
        buildInner(this);
        _indent -= 1;
        _builder.AppendIndentedLine(_indent, $"</{tag}>");
        return this;
    }

    public string Build()
    {
        _indent--;
        _builder.AppendIndentedLine(_indent, "</body>");
        _indent--;
        _builder.AppendLine("</html>");
        return _builder.ToString();
    }

    public static HtmlBuilder Create(string title)
    {
        HtmlBuilder builder = new();
        builder._builder.AppendLine("<!doctype html>");
        builder._builder.AppendLine("<html>");
        builder._builder.AppendLine(HeadBuilder.Create().AddTitle(title).Build());
        builder._indent++;
        builder._builder.AppendIndentedLine(builder._indent, "<body>");
        builder._indent++;
        return builder;
    }
}
