using Statiq.Common;
using Statiq.Core;
using Statiq.Markdown;
using Statiq.Razor;
using Statiq.Yaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines
{
    public class BlogPipeline : Pipeline
    {
        public BlogPipeline()
        {
            this.InputModules = new ModuleList()
            {
                new ReadFiles("./**/*.md")
            };

            this.ProcessModules = new ModuleList()
            {
                    new ExtractFrontMatter(new ParseYaml()),
                    new RenderMarkdown()
                        .UseExtensions(),

                    new Harrison314Blog.Pipelines.Modules.Typography(),
                    new Harrison314Blog.Pipelines.Modules.UseHighilerModule(),

                    new ExecuteSwitch("UseNewPath")
                     .Case(true, new SetDestination(".html"))
                     .Default(new SetDestination(Config.FromDocument<NormalizedPath>(UseLegacyPaths))),

                    new SetMetadata("IsBlogPost", Config.FromValue<bool>(true)),
                    new SetMetadata("IssueUrl", Config.FromDocument<string>(GithubIssePath)),
            };

            this.PostProcessModules = new ModuleList()
            {
                new RenderRazor(),
                new ProcessShortcodes(),
                new  Statiq.Highlight.HighlightCode()
                        .WithMissingLanguageWarning(true),
                new ExecuteIf(BlogKeys.MinifyOutput,
                    new Statiq.Minification.MinifyHtml())
            };

            this.OutputModules = new ModuleList()
            {
                new WriteFiles()
            };
        }

        private static NormalizedPath UseLegacyPaths(IDocument document, IExecutionContext context)
        {
            string destination = document.GetString("Destination");
            string originalFile = System.IO.Path.GetFileNameWithoutExtension(destination);

            return new NormalizedPath(string.Concat(originalFile, ".html"), PathKind.Relative);
        }

        private static string GithubIssePath(IDocument document)
        {
            string issueUrl = string.Concat("https://github.com/harrison314/harrison314.github.io/issues/new?title=",
                          Uri.EscapeDataString(document.GetString("Title")),
                          "&body=",
                          Uri.EscapeDataString(document.GetString("Destination")));

            return issueUrl;
        }
    }
}
