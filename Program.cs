using System.Net;
using System.Text;
using System.Text.Json;
using AngleSharp;
using CrawlerHttpCLI;

string year = "2022", modelName = "Aviator", modelId = "6807";

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("[ (Ford Book Crawler) VehicleID By YearModel ]");
var data = await CrawlerRequests.GetVehicles(modelId);
Console.WriteLine(data);

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine($"[ (Ford Book Crawler) Book for vehicleId={modelId}. {year} {modelName} ]");
var fordBook = await CrawlerRequests.GetPublication(modelId, year, modelName);
Console.WriteLine(JsonSerializer.Serialize(fordBook, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine($"[ (Ford Book Crawler) Table of Content ]");
var procedureIds = await CrawlerRequests.GetProcedureList(fordBook);
Console.WriteLine($"OemProcedureIds = [{string.Join(", ", procedureIds.Take(50))}, ...]");
Console.WriteLine($"Length = {procedureIds.Count}");

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine($"[ (Ford Book Crawler) Procedure Css ]");
var css = await CrawlerRequests.GetCssAsync(
    "https://www.fordtechservice.dealerconnection.com/Content/pts.desktop.publications.css");
Console.WriteLine(css[0 .. 200]);
Console.WriteLine($"Length = {css.Length}");

internal abstract class CrawlerRequests
{
    private const string FordTechServiceUrl = "https://www.fordtechservice.dealerconnection.com";

    public static async Task<string> GetVehicles(string modelId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"{FordTechServiceUrl}/VehicleId/LoadVehicleIDByYearmodel?vehicleId={modelId}");

        request.Headers.Add("Host", "www.fordtechservice.dealerconnection.com");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:143.0) Gecko/20100101 Firefox/143.0");
        request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Sec-Fetch-Dest", "empty");
        request.Headers.Add("Sec-Fetch-Mode", "cors");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Priority", "u=0");
        request.Headers.Add("TE", "trailers");
        request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        request.Headers.Add("x-dtpc", "25$116129126_957h19vMPDOKWQCMCRQAEAFKKAVAVFPVDCQPAMK-0e0");
        request.Headers.Add("x-dtreferer", "https://www.fordtechservice.dealerconnection.com/");
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/Home/VehicleMenu");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseText = await response.Content.ReadAsStringAsync();
        return responseText;
    }

    public static async Task<FordBookPub> GetPublication(string vehicleId, string year, string modelName)
    {
        var url = $"{FordTechServiceUrl}/publication/Book/workshop/{vehicleId}?model={modelName}&modelYear={year}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        // request.Headers.Add("Referer", FordTechServiceUrl);
        request.Headers.Add("Host", "www.fordtechservice.dealerconnection.com");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:143.0) Gecko/20100101 Firefox/143.0");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Sec-Fetch-Dest", "iframe");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Priority", "u=4");
        request.Headers.Add("TE", "trailers");
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        var fordBookResponse = FordBookPub.Parse(html);
        return fordBookResponse;
    }

    private static async Task<string> GetBookTableOfContent(string url, string commandJson)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Origin", FordTechServiceUrl);
        request.Headers.Add("Referer", FordTechServiceUrl);
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Accept-Encoding", "*");
        request.Content = new StringContent(commandJson, Encoding.UTF8, System.Net.Mime.MediaTypeNames.Application.Json);

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        return content;
    }

    public static async Task<string> GetCssAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("Host", "www.fordtechservice.dealerconnection.com");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:143.0) Gecko/20100101 Firefox/143.0");
        request.Headers.Add("Accept", "text/css,*/*;q=0.1");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/");
        request.Headers.Add("Sec-Fetch-Dest", "style");
        request.Headers.Add("Sec-Fetch-Mode", "no-cors");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient(new HttpClientHandler{ AutomaticDecompression = DecompressionMethods.All });
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var css = await response.Content.ReadAsStringAsync();

        return css;
    }

    public static async Task<List<string>> GetProcedureList(FordBookPub book)
    {
        var baseUrl = book.PublicationBookTreeAndCoverBase;
        var url = $"{baseUrl}?bookTitle={book.BookTitle}&WiringBookTitle={book.WiringBookTitle}";
        var command = new
        {
            book.VehicleId,
            book.ModelYear,
            Channel = book.ChannelId,
            Book = book.BookCode,
            book.BookType,
            book.Country,
            book.Language,
            book.ContentMarket,
            book.ContentLanguage,
            book.LanguageOdysseyCode,
            book.Category,
            book.CategoryDescription,
            book.WiringBookCode,
            FromPageBase = book.Origin,
            book.IsMobile
        };
        var json = JsonSerializer.Serialize(command, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var content = await GetBookTableOfContent(url, json);

        var config = Configuration.Default;
        using var context = BrowsingContext.New(config);
        using var doc = await context.OpenAsync(req => req.Content(content));
        var wsmTree = doc.GetElementById("wsm-tree");
        if (wsmTree is null)
        {
            throw new Exception("Unable to find the wsm-tree element");
        }

        var procedureIds = await GetListOfProcedures($"<html>{wsmTree.OuterHtml}</html>");
        return procedureIds;
    }

    private static async Task<List<string>> GetListOfProcedures(string tocHtml)
    {
        using var context = BrowsingContext.New(Configuration.Default);
        using var doc = await context.OpenAsync(req => req.Content(tocHtml));

        // start crawling procedures
        var links = doc.QuerySelectorAll("#treeNodesDiv > .tree > .branch")
            .Where(n => !n.QuerySelector("span")!.TextContent.Contains("quick links", StringComparison.InvariantCultureIgnoreCase))
            .SelectMany(n => n.QuerySelectorAll("a[data-for][data-procuid]"))
            .ToList();

        var procedureIds = links.Select(link => link.GetAttribute("data-for")).ToList();

        return procedureIds!;
    }
}


internal abstract class CookieProvider
{
    private static string _cachedCookies = "";
    public static string Get()
    {
        if (_cachedCookies != "") return _cachedCookies;

        var s = File.ReadAllText("./browser");
        var cookieState = JsonSerializer.Deserialize<CookieState>(s, new JsonSerializerOptions { PropertyNameCaseInsensitive = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
        if (cookieState == null)
        {
            throw new Exception("Could not deserialize cookies");
        }
        var acceptedNames = new[] { "Ford.TSO.PTSSuite", "ASP.NET_SessionId", "RT", "TPS%2DPERM", "PERSISTENT", "PREFERENCES", "TPS%2DMEMBERSHIP", "AKA_A2", "bm_mi", "bm_sv", "ak_bmsc", "dtCookie", "SSSCParameter", "rxVisitor", "dtSa", "rxvt", "dtPC" };
        var selectedCookies = cookieState.Cookies.Where(c => acceptedNames.Contains(c.Name) && c.Domain.Contains(".dealerconnection.com")).ToList();
        var cookieValue = string.Join("; ", selectedCookies.Select(c => $"{c.Name}={c.Value}"));
        _cachedCookies = cookieValue;
        return cookieValue;
    }
}


public class CookieState
{
    public Cookie[] Cookies { get; set; } = null!;
}

public class Cookie
{
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string Domain { get; set; } = null!;
    public string Path { get; set; } = null!;
    public double Expires { get; set; }
    public bool HttpOnly { get; set; }
    public bool Secure { get; set; }
    public string SameSite { get; set; } = null!;
}

