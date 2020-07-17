namespace PinkUmbrella.Util
{
    public static class HtmlStringExtensions
    {
        public static string HtmlExtractElement(this string html, string elem)
        {
            var start = html.IndexOf(">", html.IndexOf("<" + elem)) + 1;
            var end = html.LastIndexOf("<", html.LastIndexOf("</" + elem + ">"));
            var between = html.Substring(start, end - start);
            return between;
        }

        public static string HtmlExtractBody(this string html) => html.HtmlExtractElement("body");

        public static string HtmlExtractMain(this string html) => html.HtmlExtractElement("main");
    }
}