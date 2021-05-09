using Statiq.Common;
using Statiq.Core;
using Statiq.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines
{
    public class RazorPipeline : Pipeline
    {
        public RazorPipeline()
        {
            this.InputModules = new ModuleList()
            {
                new ReadFiles("./**/{!.,!_,}*.cshtml")
            };

            this.ProcessModules = new ModuleList()
            {
                    new ExtractFrontMatter(new Statiq.Yaml.ParseYaml()),
                   
                    //new RenderMarkdown()
                    //    .UseExtensions(),
                    //new GenerateExcerpt(),
                    new SetDestination(".html")
            };

            this.PostProcessModules = new ModuleList()
            {
                new RenderRazor(),
                new ExecuteIf(BlogKeys.MinifyOutput,
                    new Statiq.Minification.MinifyHtml())
            };

            this.OutputModules = new ModuleList()
            {
                new WriteFiles()
            };
        }
    }
}
