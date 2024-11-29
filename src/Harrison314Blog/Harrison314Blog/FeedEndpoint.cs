using AspNetStatic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Harrison314Blog;
internal class FeedEndpoint
{
    internal static void RegisterRss(WebApplication app)
    {
        const string BlogTitle = "harrison314 Blog";
        const string BlogLink = "https://harrison314.github.io/";
        const string BlogDescription = "Blog jedného skeptického programátora.";
        const string BlogLang = "sk-SK";

        app.MapGet("/feed.rss", ([FromServices] IPostDictionary dictionary, [FromServices] TimeProvider timeProvider, [FromServices] IUrlManager uriManager) =>
        {
            List<object> channelContent = new List<object>();

            channelContent.Add(new XElement("title", BlogTitle));
            channelContent.Add(new XElement("link", BlogLink));
            channelContent.Add(new XElement("description", BlogDescription));
            channelContent.Add(new XElement("copyright", $"(c) 2013"));
            channelContent.Add(new XElement("pubDate", FormatRfc822(timeProvider.GetUtcNow())));
            channelContent.Add(new XElement("lastBuildDate", FormatRfc822(timeProvider.GetUtcNow()))); 
            channelContent.Add(new XElement("language", BlogLang));

            foreach (PostInfo item in dictionary.GetAll().OrderByDescending(t => t.Published))
            {
                string url = uriManager.GetAbsolute(item.HtmlName);
                channelContent.Add(new XElement("item",
                    new XElement("title", item.Properties[CommonProperties.Title]),
                    new XElement("link", url),
                    new XElement("description", item.Properties[CommonProperties.Description]),
                    new XElement("guid", url, new XAttribute("isPermaLink", "false")),
                    new XElement("pubDate", FormatRfc822(item.Published))

                    ));
            }

            XDocument document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
            document.Add(new XElement("rss",
                new XAttribute("version", "2.0"),
                new XElement("channel",
                content: channelContent.ToArray()
                )));

            using MemoryStream ms = new MemoryStream();
            using (TextWriter textWriter = new StreamWriter(ms, System.Text.Encoding.UTF8, 2048, true))
            {
                using XmlWriter xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings()
                {
                    Indent = true
                });
                document.Save(xmlWriter);
                xmlWriter.Flush();
                textWriter.Flush();
            }

            return Results.Bytes(ms.ToArray(), "application/xml+rss");
        });
    }

    internal static void RegisterSitemap(WebApplication app)
    {

        app.MapGet("/sitemap.xml", ([FromServices] IStaticResourcesInfoProvider provider, [FromServices] IUrlManager uriManager) =>
        {
            List<object> usrlSet = new List<object>();

            foreach(PageResource page in provider.PageResources)
            {
                string file = page.OutFile ?? page.Route;
                string absolute = uriManager.GetAbsolute(file);
                usrlSet.Add(new XElement("url",
                    new XElement("loc", absolute),
                    new XElement("lastmod", page.LastModified.ToString("yyyy-MM-dd"))
                    ));
            }
           

            XDocument document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"));
            document.Add(new XElement("urlset",
                content: usrlSet.ToArray()
                ));

            using MemoryStream ms = new MemoryStream();
            using (TextWriter textWriter = new StreamWriter(ms, System.Text.Encoding.UTF8, 2048, true))
            {
                using XmlWriter xmlWriter = XmlWriter.Create(textWriter, new XmlWriterSettings()
                {
                    Indent = true
                });
                document.Save(xmlWriter);
                xmlWriter.Flush();
                textWriter.Flush();
            }

            return Results.Bytes(ms.ToArray(), "application/xml+sitemap");
        });
    }

    private static string FormatRfc822(DateTime date)
    {
        return date.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FormatRfc822(DateTimeOffset date)
    {
        return date.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string FormatRfc822(DateOnly date)
    {
        return FormatRfc822(new DateTime(date, new TimeOnly(0, 0), DateTimeKind.Utc));
    }
}
