using AspNetStatic;
using System.IO;

namespace Harrison314Blog;

internal static class StaticResourcesInfoProviderExtensions
{
    public static StaticResourcesInfoProvider AddContent(this StaticResourcesInfoProvider provider)
    {
        provider.Add(new PageResource("/") { OutFile = "index.html", LastModified = DateTime.UtcNow });
        provider.Add(new PageResource("/DevelopingAll") { OutFile = "DevelopingAll.html", LastModified = DateTime.UtcNow });
        provider.Add(new PageResource("/GeneralAll") { OutFile = "GeneralAll.html", LastModified = DateTime.UtcNow });
        provider.Add(new PageResource("/All") { OutFile = "All.html", LastModified = DateTime.UtcNow });
        provider.Add(new PageResource("/Portfolio") { OutFile = "Portfolio.html", LastModified = DateTime.UtcNow });


        provider.Add(new BinResource("/feed.rss") { OptimizerType = OptimizerType.None });
        provider.Add(new BinResource("/sitemap.xml") { OptimizerType = OptimizerType.None });
        return provider;
    }

    public static StaticResourcesInfoProvider AddMarkdawns(this StaticResourcesInfoProvider provider, out IPostDictionary postDictionary)
    {
        PostDictionary dictionary = new PostDictionary();

        EnumerationOptions enumerationOptions = new EnumerationOptions()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            MatchType = MatchType.Simple,
            RecurseSubdirectories = true
        };
        foreach (string path in Directory.GetFiles("./Post", "*.md", enumerationOptions))
        {
            string fileName = Path.GetFileName(path);
            string htmlName = Path.ChangeExtension(fileName, ".html");

            PostInfo info = dictionary.AddInfo(fileName, htmlName, Path.GetFullPath(path));
            provider.Add(new PageResource("/md/" + fileName)
            {
                OutFile = htmlName,
                LastModified = new DateTime(info.LastModified, new TimeOnly(0, 0), DateTimeKind.Utc)
            });
        }

        postDictionary = dictionary;
        return provider;
    }
}
