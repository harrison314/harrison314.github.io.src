using Statiq.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314Blog.Pipelines.Modules
{
    internal static class ContentProviderExtensions
    {
        public static async Task<string> GetString(this IContentProvider contentProvider, Encoding encoding = null)
        {
            if (encoding is null)
            {
                encoding = Encoding.UTF8;
            }

            using Stream stream = contentProvider.GetStream();
            using TextReader tr = new StreamReader(stream, encoding, false, -1, true);
            return await tr.ReadToEndAsync();
        }
    }
}
