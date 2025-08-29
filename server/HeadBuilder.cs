using System.Text;

namespace server;

public sealed class HeadBuilder
{
    private readonly StringBuilder _builder = new(128);
    private readonly int _indent = 1;

    private HeadBuilder() { }

    public HeadBuilder AddTitle(string title)
    {
        _builder.AppendIndentedLine(_indent + 1, $"<title>{title}</title>");
        return this;
    }

    public string Build()
    {
        _builder.AppendIndentedLine(_indent, $"</head>");
        return _builder.ToString();
    }

    public static HeadBuilder Create()
    {
        HeadBuilder builder = new();
        builder._builder.AppendIndentedLine(builder._indent, "<head>");
        return builder.AddCommon();
    }

    private HeadBuilder AddCommon()
    {
        _builder.AppendIndentedLine(_indent + 1, "<meta charset=\"UTF-8\" />");
        return this;
    }
}
