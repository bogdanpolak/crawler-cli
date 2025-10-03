using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using AngleSharp;
using CrawlerHttpCLI;

string year = "2022", modelName = "Aviator", modelId = "6807";

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("[ (Ford Book Crawler) VehicleID By YearModel ]");
var json = await BookCrawlerRequests.GetVehicles(modelId);
Console.WriteLine($"{json}\nLength = {json.Length}");
Utils.SaveToFile(json, "01 VehicleInfo.json");


Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine($"[ (Ford Book Crawler) Book for vehicleId={modelId}. {year} {modelName} ]");
var vehiclePublicationHtml = await BookCrawlerRequests.GetVehiclePublication(modelId, year, modelName);
Console.WriteLine($"{vehiclePublicationHtml[ .. 500]}\nLength = {vehiclePublicationHtml.Length}");
Utils.SaveToFile(vehiclePublicationHtml, "02 VehiclePublication.html");

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("[ (Ford Book Crawler) Table of Content ]");
FordBookPub fordBook = FordBookPub.Parse(vehiclePublicationHtml);
var url = $"{fordBook.PublicationBookTreeAndCoverBase}" +
          $"?bookTitle={fordBook.BookTitle}&WiringBookTitle={fordBook.WiringBookTitle}";
var command = Processing.BuildGetTableOfContentCommand(fordBook);
var tableOfContentHtml = await BookCrawlerRequests.GetBookTableOfContent(url, command);
Console.WriteLine($"{tableOfContentHtml[ .. 500]}\nLength = {tableOfContentHtml.Length}");
Utils.SaveToFile(tableOfContentHtml, "03 TableOfContent.html");
var procedureIds = await Processing.ExtractFordProcedureIds(tableOfContentHtml);
Console.WriteLine($"OemProcedureIds = [{string.Join(", ", procedureIds.Take(50))}, ...] Length = {procedureIds.Count}");

Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("[ (Ford Book Crawler) Procedure Css ]");
var cssDesktop = await BookCrawlerRequests.GetCssAsync(
    "https://www.fordtechservice.dealerconnection.com/Content/pts.desktop.publications.css");
var cssLoader = await BookCrawlerRequests.GetCssAsync(
    "https://www.fordtechservice.dealerconnection.com/Content/Loader.css");
Console.WriteLine($"{cssDesktop[ .. 100]}\nLength = {cssDesktop.Length}");
Console.WriteLine($"{cssLoader[ .. 100]}\nLength = {cssLoader.Length}");
Utils.SaveToFile(cssDesktop, "04 pts.desktop.publications.css");
Utils.SaveToFile(cssDesktop, "04 Loader.css");


Console.WriteLine("\n--------------------------------------------------------------------------");
Console.WriteLine("Bulletins");
Console.WriteLine("--------------------------------------------------------------------------");
Console.WriteLine("[ (Ford Bulletin Crawler) Bulletin Table of Content for Vehicle ]");
var bulletinTocHtml = await BulletinCrawlerRequests.GetVehicleToCHtml(year, modelName, modelId);
Console.WriteLine($"{bulletinTocHtml[ .. 500]}\nLength = {bulletinTocHtml.Length}");
Utils.SaveToFile(bulletinTocHtml, "11 bulletinToc.html");
var bulletinsIds = await Processing.GetBulletinsIds(bulletinTocHtml);
Console.WriteLine($"BulletinsIds = [{string.Join(", ", bulletinsIds.Take(50))}, ...] Length = {bulletinsIds.Count}");

Console.WriteLine("[ (Ford Bulletin Crawler) Bulletin Content ]");

Console.WriteLine("--------------------------------------------------------------------------");
var bulletinId = "24-7133"; // bulletinsIds[1];
Console.WriteLine($"[ (Ford Bulletin Crawler) Bulletin Table of Content for Vehicle ] BulletinId = {bulletinId}");
var bulletinHtml = await BulletinCrawlerRequests.GetTsbContent(bulletinId);
Console.WriteLine($"{bulletinHtml[ .. 500]}\nLength = {bulletinHtml.Length}");
Utils.SaveToFile(bulletinHtml, $"12 bulletin-{bulletinId}.html");

internal abstract class BookCrawlerRequests
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
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/Home/VehicleMenu");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient(new HttpClientHandler{ AutomaticDecompression = DecompressionMethods.All });
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var responseText = await response.Content.ReadAsStringAsync();
        return responseText;
    }

    public static async Task<string> GetVehiclePublication(string vehicleId, string year, string modelName)
    {
        var url = $"{FordTechServiceUrl}/publication/Book/workshop/{vehicleId}?model={modelName}&modelYear={year}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("Host", "www.fordtechservice.dealerconnection.com");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:143.0) Gecko/20100101 Firefox/143.0");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        return html;
    }

    public static async Task<string> GetBookTableOfContent(string url, string commandJson)
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

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient(new HttpClientHandler{ AutomaticDecompression = DecompressionMethods.All });
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var css = await response.Content.ReadAsStringAsync();

        return css;
    }
}


internal abstract class BulletinCrawlerRequests
{
    private const string FordTechServiceUrl = "https://www.fordtechservice.dealerconnection.com";

    // https://www.fordtechservice.dealerconnection.com/vdirsnet/TSB_SSM/DisplayArticles.aspx
    // ?year=2024&
    // model=E-450&
    // masterId=7077
    public static async Task<string> GetVehicleToCHtml(string year, string modelName, string modelId)
    {
        var url = $"{FordTechServiceUrl}/vdirsnet/TSB_SSM/DisplayArticles.aspx?year={year}&model={modelName}&masterId={modelId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("Host", "www.fordtechservice.dealerconnection.com");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:143.0) Gecko/20100101 Firefox/143.0");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/");
        request.Headers.Add("Sec-Fetch-Dest", "iframe");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Priority", "u=4");
        request.Headers.Add("TE", "trailers");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        return html;
    }

    // Crawl TSBs
    // https://www.fordtechservice.dealerconnection.com/vdirsnet/TSB_SSM/DisplayArticle.aspx
    // ?id=24-7068
    public static async Task<string> GetTsbContent(string tsbId)
    {
        var url = $"{FordTechServiceUrl}/vdirsnet/TSB_SSM/DisplayArticle.aspx?id={tsbId}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Add("Host", "www.fordtechservice.dealerconnection.com");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:143.0) Gecko/20100101 Firefox/143.0");
        request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
        request.Headers.Add("Accept-Encoding", "gzip, deflate, br, zstd");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Upgrade-Insecure-Requests", "1");
        request.Headers.Add("Referer", "https://www.fordtechservice.dealerconnection.com/");
        request.Headers.Add("Sec-Fetch-Dest", "iframe");
        request.Headers.Add("Sec-Fetch-Mode", "navigate");
        request.Headers.Add("Sec-Fetch-Site", "same-origin");
        request.Headers.Add("Sec-Fetch-User", "?1");
        request.Headers.Add("Priority", "u=4");
        request.Headers.Add("TE", "trailers");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient(new HttpClientHandler{ AutomaticDecompression = DecompressionMethods.All, AllowAutoRedirect = false});
        var response = await httpClient.SendAsync(request);
        var doContinue = true;
        while (doContinue && response.StatusCode == HttpStatusCode.Redirect)
        {
            var nextUri = response.Headers.Location;
            if (nextUri == null)
            {
                throw new Exception("Location not found in response.");
            }
            var baseUrl = request.RequestUri?.GetLeftPart(UriPartial.Authority);
            var url2 = nextUri.IsAbsoluteUri ? nextUri.OriginalString : baseUrl + nextUri.OriginalString;
            Console.WriteLine($"Redirecting to {url2}");

            using var nextRequest = await request.CloneAsync();
            nextRequest.RequestUri = new Uri(url2);

            try
            {
                response = await httpClient.SendAsync(nextRequest);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Redirect failed: {e.Message}");
                doContinue = false;
            }
        }

        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        return html;
    }
}

internal abstract class Processing
{
    public static string BuildGetTableOfContentCommand(FordBookPub book)
    {
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
        return json;
    }

    public static async Task<List<string>> ExtractFordProcedureIds(string tableOfContentHtml)
    {
        using var context = BrowsingContext.New(Configuration.Default);
        using var doc1 = await context.OpenAsync(req => req.Content(tableOfContentHtml));
        var wsmTree = doc1.GetElementById("wsm-tree");
        if (wsmTree is null)
        {
            throw new Exception("Unable to find the wsm-tree element");
        }

        var links = wsmTree.QuerySelectorAll("#treeNodesDiv > .tree > .branch")
            .Where(n => !n.QuerySelector("span")!.TextContent.Contains("quick links",
                StringComparison.InvariantCultureIgnoreCase))
            .SelectMany(n => n.QuerySelectorAll("a[data-for][data-procuid]"))
            .ToList();

        var procedureIds = links.Select(link => link.GetAttribute("data-for")).ToList();

        return procedureIds!;
    }


    public static async Task<List<string>> GetBulletinsIds(string bulletinTocHtml)
    {
        using var context = BrowsingContext.New(Configuration.Default);
        using var document = await context.OpenAsync(req => req.Content(bulletinTocHtml));

        var articles = document.QuerySelectorAll("#articles .article a[href]");
        var bulletinsIds = new List<string>();
        foreach (var article in articles)
        {
            var href = article.GetAttribute("href");
            if (!string.IsNullOrEmpty(href) && href.Contains('?'))
            {
                var queryParamsString = href.Split('?')[1];
                var parsed = HttpUtility.ParseQueryString(queryParamsString);
                var tsbId = parsed["id"];
                if (!string.IsNullOrEmpty(tsbId))
                {
                    bulletinsIds.Add(tsbId);
                }
            }
        }

        return bulletinsIds;
    }
}

internal abstract class Utils
{
    private static string _outputFolder = "httpResponses";
    public static void CleanupOutputFolder()
    {
        if (Directory.Exists(_outputFolder))
            Directory.Delete(_outputFolder, true);
    }

    public static void SaveToFile(string content, string fileName)
    {
        Directory.CreateDirectory(_outputFolder);
        File.WriteAllText(Path.Combine(_outputFolder,fileName) , content);
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

