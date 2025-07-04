using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace Gemini;

public record Part(string Text);
public record Content(string? Role, IEnumerable<Part> Parts);
public record SystemInstruction(IEnumerable<Part> Parts);

public class GenerateContentRequest(IEnumerable<Content> contents, SystemInstruction? systemInstruction)
{
    public SystemInstruction? SystemInstruction { get; } = systemInstruction;
    public IEnumerable<Content> Contents { get; } = contents;
}

public record Candidate(Content Content);
public record GenerateContentResponse(IEnumerable<Candidate>? Candidates);

public static class Gemini
{
    private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string Model = "gemini-2.0-flash";
    private const string GenerateContent = "generateContent";

    public static async Task<string> GenerateTextAsync(HttpClient client, string text, string systemInstruction)
    {
        var collection = HttpUtility.ParseQueryString(string.Empty);
        collection.Add("key", Environment.GetEnvironmentVariable("T_T"));
        var uri = $"{BaseUrl}/{Model}:{GenerateContent}?{collection}";
        JsonSerializerOptions options = new(JsonSerializerOptions.Default)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };
        var request = new GenerateContentRequest(
            [new("user", [new(text)])],
            new SystemInstruction([new("don't use lists"), new("use only imaginary facts"), new(systemInstruction)])
        );
        var response = await client.PostAsJsonAsync(uri, request, options);
        try
        {
            var generateContentResponse = await response.Content.ReadFromJsonAsync<GenerateContentResponse>(options);
            return generateContentResponse?.Candidates?.Single().Content.Parts.Single().Text
                   ?? $"Error: {response}";
        }
        catch (Exception e)
        {
            return $"Error: {e}: {e.Message}{Environment.NewLine}{e.StackTrace}";
        }
    }
}