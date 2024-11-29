using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace Harrison314Blog;

public class PostDictionary : IPostDictionary
{
    private readonly Dictionary<string, PostInfo> postInfos;
    public PostDictionary()
    {
        this.postInfos = new Dictionary<string, PostInfo>();
    }

    public PostInfo GetPostInfo(string fileName)
    {
        return this.postInfos[fileName];
    }

    public PostInfo AddInfo(string fileName, string htmlName, string path)
    {
        string[] chunks = File.ReadAllText(path).Split("---", 2);
        Dictionary<string, string> properties = new Dictionary<string, string>();
        if (chunks.Length == 2)
        {
            IDeserializer serializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();

            string yaml = chunks[0].Trim();
            properties = serializer.Deserialize<Dictionary<string, string>>(yaml);
        }

        PostInfo info = new PostInfo(fileName, path, htmlName, properties);
        this.postInfos.Add(fileName, info);

        return info;
    }

    public IEnumerable<PostInfo> GetAll()
    {
        return this.postInfos.Values;
    }

    public string ReadContent(string fileName)
    {
        string[] chunks = File.ReadAllText(this.postInfos[fileName].FullPath).Split("---", 2);
        return chunks.Length == 2 ? chunks[1] : chunks[0];
    }
}