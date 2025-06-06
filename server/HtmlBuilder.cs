using System.Text;

namespace server;

public sealed class HtmlBuilder
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

    public HtmlBuilder WithTag(string tag, Action<HtmlBuilder> buildInner, string? style = null, string? attributes = null)
    {
        AppendLine($"<{tag} style='{style ?? string.Empty}' {attributes ?? string.Empty}>");
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