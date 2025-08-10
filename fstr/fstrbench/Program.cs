using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using fstr;

BenchmarkRunner.Run<LevensteinDistance>();

[DryJob]
[MemoryDiagnoser]
public class LevensteinDistance
{
    [Benchmark]
    [ArgumentsSource(nameof(Data))]
    public int LevensteinDistanceRecursive(string a, string b)
    {
        return StringMetrics.LevensteinDistance(a, b);
    }

    public IEnumerable<object[]> Data() =>
    [
        [GenerateString(10, 'a'), GenerateString(10, 'b')],
        [GenerateString(11, 'a'), GenerateString(11, 'b')],
        [GenerateString(12, 'a'), GenerateString(12, 'b')],
        [GenerateString(13, 'a'), GenerateString(13, 'b')],
        [GenerateString(14, 'a'), GenerateString(14, 'b')],
        [GenerateString(15, 'a'), GenerateString(15, 'b')],
        [GenerateString(16, 'a'), GenerateString(16, 'b')],
        [GenerateString(17, 'a'), GenerateString(17, 'b')],
    ];

    private string GenerateString(int length, char @char)
    {
        var builder = new StringBuilder(length);
        foreach (var i in Enumerable.Range(0, length))
            builder.Append(@char);
        return builder.ToString();
    }
}


