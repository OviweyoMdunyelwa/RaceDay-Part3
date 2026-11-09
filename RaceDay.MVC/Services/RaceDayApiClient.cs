using System.Net.Http.Json;
using System.Text.Json;

namespace RaceDay.MVC.Services;

// This class is the ONLY place in the MVC app that talks to the API.
// Controllers ask it to send requests; it adds the logged-in user's API cookie.
public class RaceDayApiClient
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RaceDayApiClient(IHttpClientFactory factory, IHttpContextAccessor accessor)
    {
        _factory = factory;
        _httpContextAccessor = accessor;
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string url)
    {
        var request = new HttpRequestMessage(method, url);

        // The API remembers who is logged in using its own session cookie.
        // We saved that cookie in the MVC session at login, so we send it back here.
        string? apiCookie = _httpContextAccessor.HttpContext?
            .Session.GetString("ApiSessionCookie");

        if (!string.IsNullOrWhiteSpace(apiCookie))
        {
            request.Headers.TryAddWithoutValidation("Cookie", apiCookie);
        }

        return request;
    }

    // Send a normal request (optionally with a JSON body).
    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method, string url, object? body = null)
    {
        var client = _factory.CreateClient("RaceDayApi");
        using var request = CreateRequest(method, url);

        if (body != null)
            request.Content = JsonContent.Create(body);

        return await client.SendAsync(request);
    }

    // Send a file (used later for banner and profile pictures).
    public async Task<HttpResponseMessage> UploadFileAsync(
        string url, IFormFile file, string fieldName = "file")
    {
        var client = _factory.CreateClient("RaceDayApi");
        using var request = CreateRequest(HttpMethod.Post, url);
        using var form = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();

        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, fieldName, file.FileName);
        request.Content = form;

        return await client.SendAsync(request);
    }

    // Turn the API's JSON reply into a C# object.
    public async Task<T?> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode) return default;
        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    // Pull the API's session cookie out of the login response.
    public static string? ExtractSessionCookie(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
            return null;

        string? header = values.FirstOrDefault(v =>
            v.StartsWith(".AspNetCore.Session=", StringComparison.OrdinalIgnoreCase));

        // Keep only "name=value" (drop Path, HttpOnly and so on).
        return header?.Split(';', 2)[0];
    }
}