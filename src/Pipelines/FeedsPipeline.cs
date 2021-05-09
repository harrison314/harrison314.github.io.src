using Statiq.Common;
using Statiq.Core;
using Statiq.Feeds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines
{
    public class FeedsPipeline : Pipeline
    {
        // TODO- doplit feed
        public FeedsPipeline()
        {
            
            this.Dependencies.AddRange(nameof(BlogPipeline));

            this.ProcessModules = new ModuleList()
            {
                //new GetPipelineDocuments(ContentType.Data),
                //new ForAllDocuments(),
                //new DebugModule(),
                //new ForEachDocument(){
                //    new ExecuteConfig(Config.FromDocument(feedDoc =>{
                //    ModuleList modules = new ModuleList();
                //        modules.Add(new DebugModule());
                //    return modules;
                //}))
                //}
                new ConcatDocuments(nameof(BlogPipeline)),
               new OrderDocuments(Config.FromDocument((x => x.GetDateTime(FeedKeys.Published))))
               .Descending(),
                new GenerateFeeds()
            };

            this.OutputModules = new ModuleList()
            {
                //new FilterDocuments(Config.FromDocument(WebKeys.ShouldOutput, true)),
                new WriteFiles()
            };
            
        }
    }
}
