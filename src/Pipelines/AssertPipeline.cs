using Statiq.Common;
using Statiq.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines
{
    public class AssertPipeline:Pipeline
    {
        public AssertPipeline()
        {
            this.Isolated = true;

            this.InputModules = new ModuleList()
            {
                new ReadFiles("./**/{!.,!_,}*.{js,css,png,jpg,jpeg,gif,ico,less,scss,html,txt}")
            };

            this.ProcessModules = new ModuleList()
            {
                // TODO: zatial tu nemam sass ani less
                //new ExecuteIf(Config.FromDocument<bool>(doc => doc.MediaTypeEquals(MediaTypes.Scss)),
                //new CompileSass()
                //),

                //new ExecuteIf(Config.FromDocument<bool>(doc => doc.MediaTypeEquals(MediaTypes.Less)),
                //new CompileLess()
                //),
            };

            this.OutputModules = new ModuleList()
            {
                new WriteFiles()
            };
        }
    }
}
