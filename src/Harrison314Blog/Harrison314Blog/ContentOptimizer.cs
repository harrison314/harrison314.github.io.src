using AspNetStatic;
using AspNetStatic.Optimizer;
using System.Text.RegularExpressions;

namespace Harrison314Blog;

internal partial class ContentOptimizer : IMarkupOptimizer
{
    private readonly DefaultMarkupOptimizer defaultMarkupOptimizer;

    [GeneratedRegex("<!--Blazor.*?-->\r?\n?", RegexOptions.Singleline)]
    private static partial Regex GetRemoveCommentRegex();

    public ContentOptimizer(DefaultMarkupOptimizer defaultMarkupOptimizer)
    {
        this.defaultMarkupOptimizer = defaultMarkupOptimizer;
    }

    public MarkupOptimizerResult Execute(string content, PageResource resource, string outFilePathname)
    {
        MarkupOptimizerResult result = this.defaultMarkupOptimizer.Execute(content, resource, outFilePathname);
        result.OptimizedContent = this.ExecuteCustomOptimization(result.OptimizedContent);

        return result;
    }

    private string ExecuteCustomOptimization(string content)
    {
        content = GetRemoveCommentRegex().Replace(content, string.Empty);

        return content;
    }
}
