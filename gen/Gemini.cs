using System.Net.Http.Json;
using System.Text.Json;
using System.Web;

namespace Gemini;

public record Part(string Text);
public record Content(string Role, IEnumerable<Part> Parts);
public record GenerateContent(IEnumerable<Content> Contents);

public record Candidate(Content Content);
public record GenerateContentResponse(IEnumerable<Candidate>? Candidates);

public static class Gemini
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string Model = "gemini-2.0-flash";
    private const string GenerateContent = "generateContent";

    public static async Task<string> GenerateTextAsync(HttpClient client, string text)
    {
        var collection = HttpUtility.ParseQueryString(string.Empty);
        collection.Add("key", Environment.GetEnvironmentVariable("T_T"));
        var uri = $"{BaseUrl}/{Model}:{GenerateContent}?{collection}";
        Console.WriteLine(uri);
        JsonSerializerOptions options = new(JsonSerializerOptions.Default)
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var response = await client.PostAsJsonAsync(uri, new GenerateContent([new("user", [new(text)])]), options);
        var generateContentResponse = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(options);
        return generateContentResponse?.Candidates?.Single().Content.Parts.Single().Text
            ?? $"Error: {response}";
    }
}