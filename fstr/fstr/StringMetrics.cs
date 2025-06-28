namespace fstr;

public static class StringMetrics
{
    public static int LevensteinDistance(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        if (b.Length == 0)
        {
            return a.Length;
        }

        if (a.Length == 0)
        {
            return b.Length;
        }

        if (a[0] == b[0])
        {
            return LevensteinDistance(a[1..], b[1..]);
        }

        var first = LevensteinDistance(a[1..], b);
        var second = LevensteinDistance(a, b[1..]);
        var third = LevensteinDistance(a[1..], b[1..]);

        return 1 + Math.Min(first, Math.Min(second, third));
    }

    public static int LevensteinDistanceMatrix(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
    {
        throw new NotImplementedException(nameof(LevensteinDistanceMatrix));
    }
}