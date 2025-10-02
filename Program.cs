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

        request.Headers.Add("Origin", FordTechServiceUrl);
        request.Headers.Add("Referer", FordTechServiceUrl);
        request.Headers.Add("Accept", "*/*");
        request.Headers.Add("Accept-Language", "en-US,en;q=0.9");
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:127.0) Gecko/20100101 Firefox/127.0");
        request.Headers.Add("Connection", "keep-alive");
        request.Headers.Add("Accept-Encoding", "*");

        request.Headers.Add("Cookie", CookieProvider.Get());
        request.Version = HttpVersion.Version20;

        var httpClient = new HttpClient();
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
    private static string _cachedCookies = "Ford.TSO.PTSSuite=rKbziWtGvX-WgjsA-NfyYpSD6Sej6NGZxZqcP2Wnrd_lzsyidUds2uhz2EpDsnzJ3NCdNP2qxNKbHtJqlssbjxCEaSAQOH6weNtXjYWJUev68AcnVOwCHd9olFaeAY645f5PLhXHNxAAP2FpmrtNysE9cfG6y6yAn4MJ6sKztEEM_J9uKWtsbsRNiwyv23kCvLi3kF6NOXr2qker4M4HtD-7yI6gUUDJW2NW7uzNxq1ZZgo9NNaASzAMiMvLIpfKybT0IbJWIfd8vW3mtUxCTYiph9DWul9ck07yNxVYZ6w18u1RUE58asNZcmXWGWkVsAuxJOYddmqGPz_228zgVClIXkn3_SUMlZR37mQQR-ogyA9pntzyngjkDrvmVFRPiWN5QLOQwzJVTQsw_UMJbaFjaZ36WZz7kLICLUu0G9nQc67fFoyMNJfpjdcjOtEL91dGmQNsBiZYDEFnCeA4UU1EkqMCrxooaHKzJPn0VyqkynDngNyhP9IuFPas47X7PZueIGESwZte_FiMYctL5NdjpWGFimlkGeK2zCq0TROPznCFKDnTSXWrEWg-l229YnhPl0-s1YT2tvFQiMoo433K3--5Y81HLyX3FXliNPwWJaUHGbowlQ6HFPwvPdN6IpZoCqB5P1Y_RZPCRiZeaQILc2BtS4qagWJ4q5P9BrNXRHQnY9-YPNEhDrRVlewFqzHKkb9KktPLCtMYjzoJoPUM4u0iyGIVG_kLfDnqsoz5ireptaw7f0gKzzIHApeKqUaeAwLCvFoaKPygVTs6IGwDK31ks578WG59-aRjZ2A1EHVIkLNNfIS2Azb26zhSvQp6RcZ8rsR4hwlUKOlLU-VSNFBva5pGAT2i5P1fZZBdJ9_WHp7Tn8MfP1mEgi11dW4TLU4W8LupF6hkvWkX0jJkUM0ZqeQWZCCncA_VJ0vSpam5b9R7JIgBvxLq-HhzzIooBMnt8upvgjcF70zJA_x8FAXF-0pAFn7oHl042b8fbdDh9jJmECdT5Y5qlSLzQxzBLen_2J9QZ-5Mgm2llI3mpb3imaD_RFXHjS-UL-Kj8vFNyfla0Xjn87JDp3poDQu5Yjx8O1Qi3F4VZqBwpsqb7MQA5Xiu9WD-qEgEhRvZtU39xI2l67BrM4Au75KBmn12CJcjDbwubNYvU8dIO7J7z8NSrSfU0uhr6uB47Mt_v9c92izdT4uIdbbeJzUEyMejoThHwbnYF4p4yW9yETpD3RpV3HuzPdC7SEAy9Pgr10Tyi7Whbeep0g68T5sD0MUIULsBJIh1LdA0QiD-ywHiKPT60OKPHb2N4_zgoNf2mp4IVdvRUYL-hGIKFIwrfXAEjZhrFYSzRjjhgw51PufOI8qPsKI0M_a2ZwPWd1p-P1PMgNrYzK3LYMXxzxaWJYPTOpUnHbi1LWPbWmcY_fIxPi-tKOwFA3wRrYvkltrYAxXPRjs8ph5sc-X77h-9vZ8iLjRPIHq11MOdsN5IDOZ_1OAk8BdW0gh2lAn1EsHAcl-8F7-jENuT3k7v-IveMmfuwm2etAO-ct2r6PpH2toBszl8HPrkR3lHaVj-GeYt45hQSksjMM6WYv4cvU5QNa8y21ye7a9Xgx44E5IoEsw8qtZVGltXrV5vDV3xVK7g9capKlhKg9jgXZSboMvRGmKkNN_7zHATPwz3e0sk1-yw_bG9SSjVuMmL1582YHqWCrY9KABVnkUY9jJdYQrumQO3q6t_JcPJOr6O52Po8sVUz390gF3gZVOIY1-fbqKvJB1Wc3foXuD1NUduce7T-9z1FdeBbCqp28y_bbeQcY3tcx4wMPFuLPlo_E21r5FEYey3uGWaBsr76QDitTTyqpu1H7Fa0Y9d9lqZyVAXZYUJcopfHrWJXDOTUqQ9X50-xHBZ7UpfBCUpnpk50YV6y7SIh9iWcbeVU69b9FhvUsSNU-EiQBFjX3vsOHorvotNeySIqu7e3zyPavYFQxsUdzJDdtFRzcX5eKmTjZfr6iucEzYClV6P9XWqC_su5LaRKZl7Oo7Ww65_SGj0LIDUSUWCAKgWh-fKugaX7PwyJLb1790DBeYHBLgkTjl-o-zQVSvN4Rek5qXgcQaSPgziP88FLFYcuTWX9oVWuwjG-iAxg1kgdd_v-XWN9RY3MskoKbr-Fp49U1d1PL33Ot-oNVnzblZWqX-g-QyUQDa1qLapWZ4ydBHBpeaAFuCTscwo4I1BrncZ1qEkejR4ci0-Spq-gN1U24AvJu_zBHlgJzt-MCTSxF-5qkvWyvr2K6yx_tBbnjuRIZTP6y4rjJ2X2by4o6Dl_biS0a6t3Jcq-bWzoVH6JWbn9FYarKLGrIx-7VMgVBBRSx1GbArPElwc7DhJBD7e76b_SNrHxanKsWDLPkP9Q7-7hQKHhGwJvqhk9p21e9KvGdANcd44zyaU2xhKQhYf8aBJYJONlaioVJQrQS8xS_ld6qIhWyDyjfpUTLSO3PhPJiVqVz3j9xY1Wqa6MNVZS0ZbYb1LJ0kwU36Udd8XtsuzrwhKy-eHqPlkd3MKVgpBMp-b6NZ75fOyYhslRa1nWJxZ63X7WXosmIMFrDXkvphNIpSykjQVkbY_MWxjZ_cFru72bFv949KWqS1R85x5sgTTK1WXd5Q64F-1B2qj98dKLX4letHOyMRDfrHQC6l2G7PWrbqBSOlIul7ksU6xLVr1kbZ0HONs-tHuqexUB8SaQLwmaLD9H-TvXp4fLbqUMqeB59dfNrPsmWry3oGnhrJb_f4_mHdxP_-02KI; ASP.NET_SessionId=zaewow1pzatbkz4io32vhv5r; RT=\"z=1&dm=www.fordtechservice.dealerconnection.com&si=691b9bc0-8afd-411a-ac42-3f0c6daf1e8b&ss=mg8ibukb&sl=1&tt=2lb&rl=1&ld=2lb\"; TPS%2DPERM=B5D6C71F4AF0A517314D80CF639BE19788FCA84C87B552B6BC983E010552A8B1413B87A0F5B29EC8D19D924B25CD5BF89F4C4EB1AE4CBEE03B1E71BB57DC675713FEF2123EE8547B29CDFC475C745DE17F27A1E3D6F5ECF831C55775A7F32C4A61B76EC43576CC68C6B39775E283B8C558E1F51B35C40E48B1CDA6E4CDF2252179C03F1FC20928CC3BF4E4C8CB12F4BE988040866697B16B9B6E0148A47026CEE0966D8C6721FB28589066C5C5E6ED7E0228F7AA5CC5CCBAF6A6D0CFB1313927E417915BCA2D1E681C2B905F56EB59ABBD4F0476AD0C27CA177DD1B733DB4E000C9B0CD619C88B00A949EADDC69CF25D85C70C8A9CA45E5B51CAD7367F44C7FA55980DDEEF65A2C8CA0651C0724B0718FA03F9A82EAF4D4F0E14A9D2D552E3EFD77CD8ED0EAA884A19E9B4DB8174A7D38339B2FD232C23EFC594385E437A66B21A91F024312A376972E45C8639D9C23F733056E156F6049583D060C67E0CE144DB301D544676E9B7244DD8A12DC3E976DAB9BFAE4BD35641A84637B7694CF617AE4BC42CA3ACA22A75588C4F2F0E505C7455A7F2A6FFAEF88E4AEB79DC1EA93AA1DB507D77ECD51DED4AD832614CA91A9B6E0148A47026CEE0966D8C6721FB28ABD0CA02F3C753A058CB3CDAF3FF655CDAF1332CDC3180C6CEA23AACA84DFFB0471D7AEA759A2D27B01835BC5A59FF7CE1B5A35FC2EE9B2D1A9BE7264BEFBACC5DC2D2ECD2D96F6427D0FDCD5CCEDD890F7C7864AE7E3E59013A75B0BB181D86955E31BE4FDA44D61405F271249FE1FD322066AAEA10F32A8EB25D00FB3F970F2AFF3193CC15E119EA92E7449C55BE38BE6721080716BD3DC25E68163F56D2E80F78E22F560EFA5DEDD175B161A7D02BF24CC9928200227817410C6FBF67617E4F40357385E00240DBD9224FBE6811CD0977F3AE8690558137706BCA2938AB697D56391BF70A8F023857D65074DC1696CE83B0BA1C26A0960A66BDA39B128FD81444EDB2D38D1C0F01F7340D9E54D6DE1E1D72BD3E99190BEA095C06A3833FE71080502FCFADF7D89A9AD4635D2D651BB20BAABCA69F7756C7058A8E27A95B6044BA38C84C17CB77F9E5812E68E1998072B1072C2B3D4E65FA9099CB190B237284492C708DC3BB2A149E22D4266B9E59F8FCC68FBD96D4FC3879AB90110EE53CAC3ABC753798485D5F0A15061943F8FBC78EF269588A6CEC511EB4023DDEE6E20A6E0649B560576010A4091723C18C87971795B45F362BA23B6A709D0B340A3AA8FDF165F9908DFDCA3CFCE64AC7B287B2A9CA3EABE8189C7A6D4B1B559DBBBF83F9DB1C3573E132A7F4A2D19EEF7C5BD5BCB1AFBB76E2E17D8F10800FBD198787B7A2161E1C02476F609AE757E756F977AFAF814A302EE8138FBC23035C4DECD3900C13BFBC9A331B15AE00C63626E9466755A0D1E5C393227C92AA6EDB3AFF89E3F0E7AB25AB62A452AF964C0C1DC5113E61F3E6869E429EC539478F598270485CC120C7AD97AFACEA037C89FCF0264B9323D1C251AA23592C7E9C54BEDC93ECA7C1AC709B85C8138B25FC62B287A7F9A7BC5443EA116D5C4963CF2A1B928A1DED9D40C38E8B80; PERSISTENT=market=US&authstr=%7C&group=&cc=us&script=%2A&initlang=&region=*&flavor=RETAIL&allowedlangs=%7CEN-US%7CES-ES%7CFR-FR%7CPT-PT%7CDE%7CIT%7CRU%7C%7CEN-US%7CFR-CA%7CES-MX%7C&header=en-US%2Cen&country=USA&language=EN&version=1; PREFERENCES=language=EN-US&signature=eLZIFaW4FL/KXF/1Iw+QC2vGwRI=&country=USA&pacode=&authstr=|&flavor=RETAIL; TPS%2DMEMBERSHIP=authby=WSL&server=ITO098676&org=&dept=RETAIL&dealername=patryk.skwarko%40oeconnection.com&contentlang=EN&isolang=EN-US&contentmkt=US&language=EN&country=USA&QS_PILOT=%7c%7c&jobrole=W-&usertype=Retailer&userid=patryk.skwarko%40oeconnection.com&fin=&pacode=&pacodeGtac=&lang5Char=ENUSA&signature=6NQurFWwg5o9Vc6UuB00UY%2fHC50%3d&flavor=RETAIL&contentgrp=&authString=%7c&rightToLeft=False&marketGroup=NA&region=NA&pGroup=&isFreeSubscription=&daysUntilSubscriptionExpiry=; AKA_A2=A; bm_mi=FA43279E44835B634709A8B775B9E7EC~YAAQVTxlXwybcKCZAQAAh2u1oR0TYitMSWb79Q4a0Qcs1ns+3VzsVJ0HYcqJ7iGylw13pwj1WEJMd3HaHs9Dzdb7ZgkuQwN4/n7mYzEmXggk3+N+ZoYDMibrs1kdEpHKIrvBnb0wtCFcEZq75/SfHJulnX09nBglbT25/5hBRRF78T15Z8rmPF4BybrES1UlyZIx+ObWjdpeFatqA1kM+edhihtVzivIyfPAVI7Zc2D1SIDwAitIEvHwoTwLgk2/x3ajd2AwavK4DETqG53VJAPng0vP3EMZ4eN1BCTJoV2kz3Ivpn0e6n65LUHpqEdfvM2mQ6yD+V/5BFx7NX2FhT0U84JIov7FjU9HxrXtlVRSu2l9bvnKAw==~1; bm_sv=2475BD778605DCDD6C5AD72B405CA273~YAAQVTxlXxebcKCZAQAArWy1oR1MBR7KZFy0qF4Uf6egK0cKNSTIHCrX3nFj7FZhGxL9yvBdrWWBj3ZrGcsvkjTOPNV+2U3c7m+pkr1ApSCuorUeRbP6A0PH4UGShTQ3LD5CKdAbCEjqNZBJEJV/FyyrfZCX1ht4PsFEq1mrOZsgwT1EgV2XqgtWW5Z64verHgppoicmD9iGlbDRwb14dj6NfAS68wbIYqXd/xTqM1IbgKy3PYeCyGd1ukZJ4Ane8LTs0xGgKUoJyD82n2ypdZvimoh3zrkNFiE=~1; ak_bmsc=F5E5BF367890D557B7C08DAD05572AF5~000000000000000000000000000000~YAAQVTxlXyKbcKCZAQAAb2+1oR3MwYtHOgKhx2CGvEqCON+Lev9Ig+i+hr6gLgCFImBiMfxS4+Y304lu4LPY7S9mcvJOiiQqBC1RYWGykWyK2rAbSXKzbdf+3SvMUUU5KebOUAlvlTUUN3J2lS8zWmG+DvcOC6LoUbMWnTMHuRS0283jUvmkiWgcy7XIaDTJz1qpMZLPNCGkFYQMXJO9QeJPngv0bHTtxNZt2GKSmGc8ns8WVWUTxLTQx2vVYvKnsYnqwd1VB61iojqAbdcXgR3VbuX/aA25X0IWvGxUAcjyX7umAc2SySAw7VikTqsbmQe9mGX4+/pUP5E2/lWDzqTWj36V8HYoUFg5drp54DOHWIK9ba3E1eb+QqfjWgXwvH7nVumZQtptmM7FfvApUm8zBMYbB31iZbYCkTsFrcLUJroXMzX2kVUgLtgDc3C8+WdzOtyyT4InociNV41U+xZxjQpuM1n4AG0GtilbHvQmJOFfFHU8OpEZHCMYpcGZTqEeXCExDlWA8V7ounuHE2P0TUxQ903PIR5qaqk+hOb8ESumyE1ZW+OsSVSxzmMbQ+fGD61mE69LiYor; dtCookie=v_4_srv_35_sn_990D488C2140ED2C8C8A6C7983F07C71_perc_100000_ol_0_mul_1_app-3A79dff4109f3df103_1_rcs-3Acss_0; SSSCParameter=12+03LLG7gCPA2Y6b7K65EhxTEXGDJBI5neYgVMe97WY+Y23mEA0mNakMI57SYzm5SW+wyIVLJ8xmS8mFuVP/D+osjUPAx+pmHXcZyntj1k=; rxVisitor=1759320272750DDF53URQNU9II5JJT2POHDPL85SHGHCU; dtSa=-; rxvt=1759356446170|1759354643445; dtPC=35$154643443_449h6vPVMSNFAAFOPFJMKVGOQQUJSNAUFKFUWF-0e0";
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

