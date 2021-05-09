using Statiq.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines.Modules
{
    public class Typography : ParallelModule
    {
        private readonly Regex regex;
        public Typography()
        {
            this.regex = new Regex(@"(\s|^)(z|zo|bez|na|po|od|do|pri|pre|so|miesto|o|v|s|za|a|i|ani|aj|najprv|potom|ešte|ale|no|lež|jednako|alebo|buď|či|že|aby|čo|aký|ktorý|kde|keď|kým|kde|k|ku|čo|akoby|lebo|pretože|predsa)(\s+)((\<(i|em|strong|b)\>)?([^\p{Cc}\p{Cf}\p{Z}]+))", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            using Stream contentInputStream = input.ContentProvider.GetStream();
            using TextReader tr = new StreamReader(contentInputStream, Encoding.UTF8);
            string content = await tr.ReadToEndAsync();

            string newContent = this.UseTypography(content);

            IContentProvider contentProvider = context.GetContentProvider(newContent);
            var dccc =  input.Clone(contentProvider).Yield();

            return dccc;
        }

        private string UseTypography(string html)
        {
            string text = this.regex.Replace(html, "$1$2&#160;$4");
            text = text.Replace("…", "&#8230;");

            return text;
        }
    }
}
