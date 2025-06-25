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
