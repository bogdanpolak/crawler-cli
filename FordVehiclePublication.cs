using System.Text.RegularExpressions;

namespace CrawlerHttpCLI;

public class FordVehiclePublication
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public string PublicationBookTreeAndCoverBase { get; set; }
    public string PublicationProcContentBase { get; set; }
    public string BookCode { get; set; }
    public string VehicleId { get; set; }
    public string ModelYear { get; set; }
    public string ChannelId { get; set; }
    public string BookType { get; set; }
    public string WiringBookTitle { get; set; }
    public string WiringBookCode { get; set; }
    public string BookTitle { get; set; }
    public string Country { get; set; }
    public string Language { get; set; }
    public string ContentMarket { get; set; }
    public string ContentLanguage { get; set; }
    public string LanguageOdysseyCode { get; set; }
    public string Category { get; set; }
    public string CategoryDescription { get; set; }
    public string Origin { get; set; }
    public string IsMobile { get; set; }
    public string UserType { get; set; }
    public string Adt { get; set; }
    public string DiagTool { get; set; }
    public string Otx { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    /// <summary>
    /// Populates all the attributes based on the html string
    /// </summary>
    /// <param name="html">html of the workshop page as a string</param>
    public static FordVehiclePublication Parse(string html)
    {
        FordVehiclePublication fordVehiclePublication = new FordVehiclePublication();
        Func<Regex, int, string> regexFind = (regex, groupnum) =>
        {
            var match = regex.Match(html);
            if (!match.Success)
            {
                throw new Exception("Cannot parse workshop html to FordBookResponse");
            }
            return match.Groups[groupnum].Value;
        };

        // PublicationBookTreeAndCoverBase
        fordVehiclePublication.PublicationBookTreeAndCoverBase = regexFind(
            new Regex(@"publicationBookTreeAndCoverBase\s*=\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // PublicationProcContentBase
        fordVehiclePublication.PublicationProcContentBase = regexFind(
            new Regex(@"publicationProcContentBase\s*=\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // BookCode
        fordVehiclePublication.BookCode = regexFind(
            new Regex(@"bookcode\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // VehicleId
        fordVehiclePublication.VehicleId = regexFind(
            new Regex(@"vehicleId\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // ModelYear
        fordVehiclePublication.ModelYear = regexFind(
            new Regex(@"modelYear\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // ChannelId
        fordVehiclePublication.ChannelId = regexFind(
            new Regex(@"channelId\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // BookType
        fordVehiclePublication.BookType = regexFind(
            new Regex(@"booktype\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // WiringBookTitle
        fordVehiclePublication.WiringBookTitle = regexFind(
            new Regex(@"WiringBookTitle\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // WiringBookCode
        fordVehiclePublication.WiringBookCode = regexFind(
            new Regex(@"WiringBookCode\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // BookTitle
        fordVehiclePublication.BookTitle = regexFind(
            new Regex(@"\sbookTitle\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // Country
        fordVehiclePublication.Country = regexFind(
            new Regex(@"country\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // Language
        fordVehiclePublication.Language = regexFind(
            new Regex(@"language\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // ContentMarket
        fordVehiclePublication.ContentMarket = regexFind(
            new Regex(@"contentmarket\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // ContentLanguage
        fordVehiclePublication.ContentLanguage = regexFind(
            new Regex(@"contentlang\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // LanguageOdysseyCode
        fordVehiclePublication.LanguageOdysseyCode = regexFind(
            new Regex(@"languageOdysseyCode\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // Category
        fordVehiclePublication.Category = regexFind(
            new Regex(@"categoryId\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // CategoryDescription
        fordVehiclePublication.CategoryDescription = regexFind(
            new Regex(@"categorydesc\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // Origin
        fordVehiclePublication.Origin = regexFind(
            new Regex(@"origin\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // IsMobile
        fordVehiclePublication.IsMobile = regexFind(
            new Regex(@"isMobile\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // UserType
        fordVehiclePublication.UserType = regexFind(
            new Regex(@"usertype\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // Adt
        fordVehiclePublication.Adt = regexFind(
            new Regex(@"adt\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // DiagTool
        fordVehiclePublication.DiagTool = regexFind(
            new Regex(@"diagTool\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        // Otx
        fordVehiclePublication.Otx = regexFind(
            new Regex(@"otx\s*:\s*('|"")(.*)\1", RegexOptions.IgnoreCase),
            2
        );

        return fordVehiclePublication;
    }
}