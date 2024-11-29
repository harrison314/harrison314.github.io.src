namespace Harrison314Blog;

public interface IPostDictionary
{
    PostInfo GetPostInfo(string fileName);

    IEnumerable<PostInfo> GetAll();

    string ReadContent(string fileName);
}
