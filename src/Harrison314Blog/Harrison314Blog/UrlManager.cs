namespace Harrison314Blog;

public class UrlManager : IUrlManager
{
    private Uri uri;

    public UrlManager(string baseUrl)
    {
        this.uri = new Uri(baseUrl, UriKind.Absolute);
    }

    public string GetAbsolute(string relativeUrl)
    {
        return new Uri(this.uri, relativeUrl).ToString();
    }
}