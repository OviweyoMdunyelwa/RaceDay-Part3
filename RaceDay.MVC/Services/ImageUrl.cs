namespace RaceDay.MVC.Services;

public static class ImageUrl
{
    // Turns a stored Azure link into a link to OUR website, which asks the API for the picture.
    public static string? Proxy(string? blobUrl)
    {
        if (string.IsNullOrWhiteSpace(blobUrl)) return null;
        if (!Uri.TryCreate(blobUrl, UriKind.Absolute, out var uri)) return null;

        // First part is the container name, the rest is the picture's path.
        var parts = uri.AbsolutePath.Trim('/').Split('/', 2);
        if (parts.Length < 2) return null;

        return "/Media/Image?path=" + Uri.EscapeDataString(parts[1]);
    }
}
