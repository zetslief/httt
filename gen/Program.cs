using static Gemini.Gemini;

using var client = new HttpClient();

Console.WriteLine(await GenerateTextAsync(client, "Select random sport"));