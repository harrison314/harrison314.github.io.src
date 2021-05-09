using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines.Modules
{
    public class UseHighilerModule : ParallelModule
    {
        private readonly string metadataName;

        public UseHighilerModule(string metadataName = "UseHighiler")
        {
            this.metadataName = metadataName;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            string value = await input.ContentProvider.GetString();
            if (value.Contains("<code>", StringComparison.InvariantCultureIgnoreCase)
                || value.Contains("<code ", StringComparison.InvariantCultureIgnoreCase)
                || value.Contains("<code ", StringComparison.InvariantCultureIgnoreCase))
            {
                IDocument updated = input.Clone(new MetadataItems() { { this.metadataName, true } });
                return updated.Yield();
            }
            else
            {
                return input.Yield();
            }
        }
    }
}
