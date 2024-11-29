namespace Harrison314Blog;

public record PostInfo(string MarkdawnFile, string FullPath, string HtmlName, IReadOnlyDictionary<string, string> Properties)
{
    public DateOnly Published => DateOnly.Parse(this.Properties[CommonProperties.Published]);

    public DateOnly LastModified => this.Properties.ContainsKey(CommonProperties.Updated)
        ? DateOnly.Parse(this.Properties[CommonProperties.Updated])
        : DateOnly.Parse(this.Properties[CommonProperties.Published]);
}
