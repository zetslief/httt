// See https://aka.ms/new-console-template for more information

if (args.Length < 2)
{
    PrintError($"Not enough arguments provided. Received {args.Length}: [{string.Join(',', args)}]. Expected 2.");
    ShowHelp();
    Environment.Exit(1);
}

var a = args[0];
var b = args[1];

PrintInfo($"a: {a}");
PrintInfo($"b: {b}");

PrintInfo($"Levenstein distance is {LevensteinDistance(a, b)}");

static int LevensteinDistance(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
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

static void PrintInfo(string info)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"INFO: {info}");
    Console.ResetColor();
}

static void PrintError(string error)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"ERROR: {error}");
    Console.ResetColor();
}

static void ShowHelp()
{
}
