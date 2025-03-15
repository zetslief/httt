using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

const string baseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
const string model = "gemini-2.0-flash";
const string generateContent = "generateContent";

using var client = new HttpClient();
Console.WriteLine(await GenerateTextAsync(client, $"Hello, {model}!"));

static async Task<string> GenerateTextAsync(HttpClient client, string text)
{
    var collection = HttpUtility.ParseQueryString(string.Empty);
    collection.Add("key", Environment.GetEnvironmentVariable("T_T"));
    var uri = $"{baseUrl}/{model}:{generateContent}?{collection}";
    Console.WriteLine(uri);
    JsonSerializerOptions options = new(JsonSerializerOptions.Default)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
    var response = await client.PostAsJsonAsync(uri, new GenerateContent([new("user", [new(text)])]), options);
    Console.WriteLine(response);
    return await response.Content.ReadAsStringAsync();
}

record Part(string Text);
record Content(string Role, Part[] Parts);
record GenerateContent(IEnumerable<Content> Contents);