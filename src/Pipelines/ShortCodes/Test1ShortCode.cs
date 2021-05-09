using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines.ShortCodes
{
    public class Test1Shortcode : Shortcode // IShortcode
    {
        public Test1Shortcode()
        {

        }
        
        public override Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            ShortcodeResult result = new ShortcodeResult("Hello world! ");
            return Task.FromResult(result);
        }
    }
}
