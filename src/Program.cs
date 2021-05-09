using Statiq.App;
using Statiq.Common;
using Statiq.Feeds;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Harrison314Blog
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            // https://www.youtube.com/watch?v=rCSppLf9dIM

            return await Bootstrapper
                .Factory.Create(args)
                .AddDefaultShortcodes()
                .AddGlobCommands()
                .AddDefaults(DefaultFeatures.BuildCommands | DefaultFeatures.Logging| DefaultFeatures.Pipelines/* | DefaultFeatures.CustomCommands | DefaultFeatures.GlobCommands*/)
                .AddPipeline<Pipelines.AssertPipeline>()
                .AddPipeline<Pipelines.RazorPipeline>()
                .AddPipeline<Pipelines.BlogPipeline>()
                .AddSetting(Keys.LinkLowercase, false)
                .AddSetting(Keys.LinksUseHttps, true)
                .AddSetting(Keys.LinkHideExtensions, false)
                .AddSetting(BlogKeys.UseAbsoluteUrls, false)
                .AddSetting(Keys.Host, "harrison314.github.io")
                .AddSetting(Keys.Title, "harrison314 blog")
                .AddSetting(FeedKeys.Author, "harrison314")
                .AddSetting(FeedKeys.Description, "Blog jedného skeptického programátora.")
                .AddSetting(FeedKeys.Copyright, $"harrison314 © {DateTime.UtcNow.Year.ToString()}")
                .AddSetting(BlogKeys.MinifyOutput, false)
                .AddSetting(BlogKeys.GithubRepo, "https://github.com/harrison314/")
                .RunAsync();
        }
    }
}
