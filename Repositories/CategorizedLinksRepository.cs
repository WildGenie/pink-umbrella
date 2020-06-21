using System.Collections.Generic;
using PinkUmbrella.Models;

namespace PinkUmbrella.Repositories
{
    public class CategorizedLinksRepository
    {
        public static readonly SimpleLinkModel[] ImageLinks = {
            new SimpleLinkModel() { Url = "", Text = "" },
        };

        public static readonly SimpleLinkModel[] VideoLinks = {
            new SimpleLinkModel() { Url = "www.thestranger.com/chazlivefeed", Text = "The Stranger livestream" },
            new SimpleLinkModel() { Url = "www.twitch.tv/seattleprotest2020", Text = "SeattleProtest2020 livestream" },
        };

        public static readonly SimpleLinkModel[] CityDataLinks = {
            new SimpleLinkModel() { Url = "data.seattle.gov/City-Business/Discrimination-Case-Settlements-by-Month/qqw9-cbst", Text = "Discrimination Case Settlements" },
            new SimpleLinkModel() { Url = "data.seattle.gov/Public-Safety/Use-Of-Force/ppi5-g2bj", Text = "Use of Force" },
            new SimpleLinkModel() { Url = "data.seattle.gov/City-Business/-of-City-of-Seattle-Employees-by-Department/5aky-hgur", Text = "Salaries of City of Seattle Employees by Department" },
            new SimpleLinkModel() { Url = "data.seattle.gov/Public-Safety/SPD-Officer-Involved-Shooting-OIS-Data/mg5r-efcm", Text = "SPD Officer Involved Shooting (OIS) Data" },
        };

        public static readonly SimpleLinkModel[] CityMonitoringLinks = {
            new SimpleLinkModel() { Url = "pig.observer/seattle", Text = "DOT Cam Viewer" },
            new SimpleLinkModel() { Url = "tar1090.adsbexchange.com", Text = "Aircraft Viewer" },
            new SimpleLinkModel() { Url = "openmhz.com/system/seasimul?filter-type=group&filter-code=5ed7807b972a250025fe41f5", Text = "Public Radio Listener" },
        };

        public static readonly SimpleLinkModel[] NewsArticleLinks = {
            new SimpleLinkModel() { Url = "medium.com/@seattleblmanon3/the-demands-of-the-collective-black-voices-at-free-capitol-hill-to-the-government-of-seattle-ddaee51d3e47", Text = "Medium – The Demands of the Collective Black Voices at Free Capitol Hill to the Government of Seattle, Washington" },
        };

        public static readonly SimpleLinkModel[] DocumentationOrWikiLinks = {
            new SimpleLinkModel() { Url = "en.wikipedia.org/wiki/Capitol_Hill_Autonomous_Zone", Text = "Capitol Hill Autonomous Zone" },
            new SimpleLinkModel() { Url = "localwiki.org/seattle/", Text = "Collect, share and open the world's local knowledge" },
            new SimpleLinkModel() { Url = "en.wikipedia.org/wiki/Lists_of_killings_by_law_enforcement_officers_in_the_United_States", Text = "Lists of killings by law enforcement officers in the United States" },
        };

        public static readonly SimpleLinkModel[] OfficialPartnerLinks = {
            new SimpleLinkModel() { Url = "CHOPseattle.com", Text = "CHOP history and ongoing developments" },
            new SimpleLinkModel() { Url = "reportcops.net", Text = "Report police brutality to a public database" },
        };

        public static readonly SimpleLinkModel[] UnOfficialPartnerLinks = {
            new SimpleLinkModel() { Url = "citizenspolicecouncil.org/", Text = "Seattle Citizens Police Council" },
        };

        public static readonly SimpleLinkModel[] HealthAndWellnessLinks = {
            new SimpleLinkModel() { Url = "", Text = "" },
        };

        public static readonly SimpleLinkModel[] SocialMediaLinks = {
            new SimpleLinkModel() { Url = "https://twitter.com/seattlepd", Text = "SPD's Twitter" },
            new SimpleLinkModel() { Url = "https://twitter.com/MayorJenny", Text = "Mayor Jenny Durkan's Twitter" },
            new SimpleLinkModel() { Url = "https://twitter.com/carmenbest", Text = "SPD Chief Best's Twitter" },
        };

        public static readonly SimpleLinkModel[] FundsOrDonationLinks = {
            new SimpleLinkModel() { Url = "blacklivesseattle.org/bail-fund/", Text = "Black Lives Matter Seattle-King County Freedom Fund" },
            new SimpleLinkModel() { Url = "www.nwcombailfund.org/", Text = "Northwest Community Bail Fund" },
        };


        public readonly Dictionary<LinkCategory, SimpleLinkModel[]> CategorizedLinks = new Dictionary<LinkCategory, SimpleLinkModel[]>() {
            { LinkCategory.Image, ImageLinks },
            { LinkCategory.Video, VideoLinks },
            { LinkCategory.CityData, CityDataLinks },
            { LinkCategory.CityMonitoring, CityMonitoringLinks },
            { LinkCategory.NewsArticle, NewsArticleLinks },
            { LinkCategory.DocumentationOrWiki, DocumentationOrWikiLinks },
            { LinkCategory.OfficialPartner, OfficialPartnerLinks },
            { LinkCategory.UnOfficialPartner, UnOfficialPartnerLinks },
            { LinkCategory.HealthAndWellness, HealthAndWellnessLinks },
            { LinkCategory.SocialMedia, SocialMediaLinks },
            { LinkCategory.FundsOrDonation, FundsOrDonationLinks },
        };

        public readonly Dictionary<LinkCategory, string> CategoryTitles = new Dictionary<LinkCategory, string>() {
            // { LinkCategory.Image, "Images" },
            { LinkCategory.Video, "Videos" },
            { LinkCategory.CityData, "City Data Links" },
            { LinkCategory.CityMonitoring, "City Monitoring Links" },
            { LinkCategory.NewsArticle, "News Articles" },
            { LinkCategory.DocumentationOrWiki, "Documentation and Wiki Links" },
            { LinkCategory.OfficialPartner, "Related Sites" },
            // { LinkCategory.UnOfficialPartner, "More Related Sites" },
            // { LinkCategory.HealthAndWellness, "Health and Wellness" },
            { LinkCategory.SocialMedia, "Social Media Links" },
            { LinkCategory.FundsOrDonation, "Funds and Donation Links" },
        };
    }
}