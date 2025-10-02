namespace CrawlerHttpCLI;

#nullable enable

public static class HttpRequestMessageExtensions
{
    private static readonly char[] PathAndQueryDelimiters
        = ['/', '\\', ':', '?', '&', '=', '#', '%'];

    private static readonly string[] GraphicsFileExtensions =
        [
            // image formats:
            ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg",
            // video formats: 
            ".mp4", ".webm", ".ogg", ".avi", ".flv", ".mov",
            ".wmv", ".mkv", ".mpg", ".mpeg", ".m4v", ".3gp"
        ];

    private static readonly string[] ScriptFileExtensions =
        [
            ".js",
            ".ts",
        ];

    public static bool IsForGraphicalAsset(
        this Uri? uri, out string cleanedPath)
    {
        cleanedPath = string.Empty;

        if (uri is null) return false;

        cleanedPath =
            uri.ToString()
            .ToLower()
            .Trim();

        if (string.IsNullOrWhiteSpace(cleanedPath))
            return false;

        return
            PathAndQueryDelimiters.Any(cleanedPath.Contains)
            && GraphicsFileExtensions.Any(cleanedPath.EndsWith);
    }

    public static bool IsForGraphicalAsset(this Uri? uri)
        => uri.IsForGraphicalAsset(out _);

    public static bool IsForGraphicalAsset(
        this HttpRequestMessage? request,
        out string cleanedPath)
    {
        cleanedPath = string.Empty;
        if (request is null) return false;

        return request.RequestUri
            .IsForGraphicalAsset(out cleanedPath);
    }

    public static bool IsForGraphicalAsset(
        this HttpRequestMessage request)
        => request.IsForGraphicalAsset(out _);

    public static bool IsForScriptAsset(
        this HttpRequestMessage? request,
        out string cleanedPath)
    {
        cleanedPath = string.Empty;

        if (request is null
            || request.Method != HttpMethod.Get
            || request.RequestUri is null)
        {
            return false;
        }

        cleanedPath
            = request.RequestUri
            .ToString()
            .ToLower()
            .Trim();

        return
             ScriptFileExtensions.Any(cleanedPath.EndsWith);
    }

    public static bool IsForScriptAsset(this HttpRequestMessage request)
        => request.IsForScriptAsset(out _);

    /// <summary>
    /// Clones a request message
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static async Task<HttpRequestMessage> CloneAsync(
        this HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        try
        {
            foreach (var header in request.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content is not null)
            {
                var contentString = await request.Content.ReadAsStringAsync(cancellationToken);
                if (request.Content.Headers is not null)
                {
                    clone.Content = new StringContent(
                        content: contentString,
                        encoding: System.Text.Encoding.UTF8,
                        mediaType:
                            request.Content.Headers.ContentType?.MediaType
                            ?? "application/octet-stream");

                    foreach (var header in request.Content.Headers)
                        clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            clone.Version = request.Version;

            foreach (var property in request.Options)
                clone.Options.TryAdd(property.Key, property.Value);

            return clone;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error cloning request", ex);
        }
    }
}
