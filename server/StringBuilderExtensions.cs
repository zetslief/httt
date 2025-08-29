using System.Text;

namespace server;

public static class StringBuilderExtensions
{
    public static void AppendIndentedLine(this StringBuilder builder, int indent, string content)
    {
        for (int i = 0; i < indent; ++i)
            builder.Append('\t');
        builder.AppendLine(content);
    }
}
