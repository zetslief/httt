using System.Text;

namespace server;

public sealed class HtmlBuilder
{
    private readonly StringBuilder _builder = new(1024);
    private int _indent = 0;

    private HtmlBuilder()
    {
    }

    public HtmlBuilder AddHeader(int number, string content)
    {
        AppendLine($"<h{number}>{content}</h{number}>");
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
        _indent += 1;
        buildInner(this);
        _indent -= 1;
        AppendLine($"</{tag}>");
        return this;
    }

    private void AppendLine(string content)
    {
        for (int i = 0; i < _indent; ++i)
            _builder.Append('\t');
        _builder.AppendLine(content);
    }

    public string Build()
    {
        _indent--;
        this.AppendLine("</body>");
        _indent--;
        _builder.AppendLine("</html>");
        return _builder.ToString();
    }

    public static HtmlBuilder Create()
    {
        HtmlBuilder builder = new();
        builder.AppendLine("<!doctype html>");
        builder.AppendLine("<html>");
        builder._indent++;
        builder.AppendLine("<body>");
        builder._indent++;
        return builder;
    }
}
