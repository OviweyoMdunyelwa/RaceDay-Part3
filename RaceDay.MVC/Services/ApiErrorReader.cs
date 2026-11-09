using System.Net;
using System.Text.Json;

namespace RaceDay.MVC.Services;

public static class ApiErrorReader
{
    public static async Task<string> ReadAsync(HttpResponseMessage response, string fallback)
    {
        try
        {
            string json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                // Our own API errors look like { "message": "..." }
                if (root.TryGetProperty("message", out var msg) &&
                    msg.ValueKind == JsonValueKind.String)
                {
                    return msg.GetString()!;
                }

                // Automatic validation errors look like { "errors": { "Field": ["..."] } }
                if (root.TryGetProperty("errors", out var errors) &&
                    errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        foreach (var item in field.Value.EnumerateArray())
                        {
                            string? text = item.GetString();
                            if (!string.IsNullOrWhiteSpace(text)) return text;
                        }
                    }
                }
            }
        }
        catch
        {
            // fall through to the friendly defaults below
        }

        return response.StatusCode switch
        {
            HttpStatusCode.Forbidden => "You do not have permission to do that.",
            HttpStatusCode.Unauthorized => "Please log in again.",
            _ => fallback
        };
    }
}