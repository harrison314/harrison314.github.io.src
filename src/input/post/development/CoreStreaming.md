Published: 25.8.2016
Title: Video straming v ASP.NET Core
Menu: Video straming v ASP.NET Core
Cathegory: Dev
Description: Implementácia streamingu videa v ASP.Net Core 2.1 aplikáci do webu.
---
# Video straming v ASP.NET Core
V tomto článku ponúkam návod ako implementovať video streaming v ASP.NET Core.

Pri tvorbe midllweru som vychádzal z ukážky pre [ASP.NET Web Api](http://www.codeproject.com/Articles/820146/HTTP-Partial-Content-In-ASP-NET-Web-API-Video).

## Implementácia v ASP.NET Core
V prvom rade si vytvoríme rozhranie popisujúce služby nad video súbormi.
Prvá metóda vráti zoznam dostupných videí – slúži na ich vyhľadanie v kontrolery
(pre zobrazenie používateľského rozhrania), no pre samotný streaming nie je podstatná.

Druhá vracia inštanciu rozhrania _IVideoFile_, pomocou ktorého je možné pristupovať k video súboru, podľa ID-čka.
Vracia jeho veľkosť, meno content type, a metódy na zápis jeho obsahu do streamu.
Metóda _CopyTo_ s parametrami begin a end skopíruje do streamu iba časť súbor od – do v bajtoch.

```cs
    public interface IVideoServices
    {
        List<VideoFileInfo> FindVideoFiles();

        IVideoFile GetVideoFile(string id);
    }

    public interface IVideoFile
    {
        string ContentType
        {
            get;
        }

        string Name
        {
            get;
        }

        long Size
        {
            get;
        }

        Task CopyTo(Stream outputStream);

        Task CopyTo(Stream outputStream, long begin, long end);
    }

    public class VideoFileInfo
    {
        public string Id
        {
            get;
            internal set;
        }

        public string DisplayName
        {
            get;
            internal set;
        }

        public VideoFileInfo()
        {
        }
    }
```

Nasledujúci kód predstavuje ukážkovú implementáciu rozhrania _IVideoServices_ pre súborový systém.
V konštruktore je uvedená cesta k adresáru s videosúbormi a filter pre ich vyhľadávanie.
Podobnú službu ide spraviť napríklad pomocou [MongoDb a GridFs](https://docs.mongodb.com/manual/core/gridfs/).

```cs
    public class FileVideoServices : IVideoServices
    {
        private readonly string filter;
        private readonly string videoFolderParh;

        public FileVideoServices()
        {
            //TODO: nacitavanie z konfiguracie
            this.videoFolderParh = @"D:\example\video";
            this.filter = "*.mp4";
        }

        public List<VideoFileInfo> FindVideoFiles()
        {
            DirectoryInfo info = new DirectoryInfo(this.videoFolderParh);
            List<VideoFileInfo> result = new List<VideoFileInfo>();
            foreach (FileInfo fie in info.GetFiles("*.mp4"))
            {
                VideoFileInfo videoFile = new VideoFileInfo()
                {
                    DisplayName = fie.Name,
                    Id = fie.Name
                };
                result.Add(videoFile);
            }

            return result;
        }

        public IVideoFile GetVideoFile(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));

            string path = Path.Combine(this.videoFolderParh, id);
            FileInfo info = new FileInfo(path);
            if (info.Exists)
            {
                FileVideoFile videoFile = new FileVideoFile(info);
                return videoFile;
            }
            else
            {
                return null;
            }
        }
    }

    internal class FileVideoFile : IVideoFile
    {
        private const int bufferLenght = 1024 * 1024;

        private readonly string fullPath;

        public string ContentType
        {
            get;
            protected set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public long Size
        {
            get;
            protected set;
        }

        public FileVideoFile(FileInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            this.fullPath = info.FullName;
            this.Name = info.Name;
            this.Size = info.Length;
        }

        public async Task CopyTo(Stream outputStream)
        {
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            using (FileStream fs = new FileStream(this.fullPath, FileMode.Open))
            {
                await fs.CopyToAsync(outputStream);
            }
        }

        public async Task CopyTo(Stream outputStream, long begin, long end)
        {
            if (outputStream == null) throw new ArgumentNullException(nameof(outputStream));

            long remainingBytes = end - begin + 1;
            long position = begin;
            int count;
            byte[] buffer = new byte[bufferLenght];

            using (FileStream fs = new FileStream(this.fullPath, FileMode.Open, FileAccess.Read))
            {
                fs.Seek(begin, SeekOrigin.Begin);
                do
                {
                    if (remainingBytes > bufferLenght)
                    {
                        count = await fs.ReadAsync(buffer, 0, bufferLenght).ConfigureAwait(false);
                        await outputStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                    }
                    else
                    {
                        count = await fs.ReadAsync(buffer, 0, (int)remainingBytes).ConfigureAwait(false);
                        await outputStream.WriteAsync(buffer, 0, count).ConfigureAwait(false);
                    }

                    position += count;
                    remainingBytes -= count;
                }
                while (position <= end);
            }
        }
```

## Popis midllweru
Ako bolo uvedené v pôvodnom zdroji, tak pokiaľ požadované video neexistuje vráti sa HTTP status kód _404 Not Found_.
Ak existuje ale v hlavičke requestu sa nenachádza hlavička _Range_ vráti sa celý obsah súboru zo statusom _200 OK_ (túto časť je možné vynechať ak chceme zabrániť neoprávnenému sťahovaniu obsahu). 
Ak sa v ňom nachádza, hlavička _Range_, tak ju prečíta. 

Ak sú v nej bytové rozsahy v poriadku vráti HTTP status _206 Partial Content_ a zapíše do výstupného streamu príslušnú časť obsahu, inak vráti HTTP status _416 Requested Range Not Satisfiable_ a ukončí spojenie.

Nasledujúci kód predstavuje implementáciu samotného ASP.NET Core midllweru, ktorý sa stará o parciálne servírovanie obsahu videa pomocou _HTTP statusu 206_.

V konštante _maxTransfer_ je hodnota v bajtoch, ktorá určuje maximálnu veľkosť naraz preneseného bloku dát z videa.

```cs
    public class VideoStreamMiddleware
    {
        private readonly IVideoServices videoServices;
        private readonly RequestDelegate next;

        private const long maxTransfer = 1024 * 1024;

        public VideoStreamMiddleware(RequestDelegate next, IVideoServices videoServices)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));
            if (videoServices == null) throw new ArgumentNullException(nameof(videoServices));

            this.next = next;
            this.videoServices = videoServices;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!httpContext.Request.Path.StartsWithSegments(new PathString("/videostream")))
            {
                await this.next(httpContext);
                return;
            }

            string id = httpContext.Request.Query["file"];
            IVideoFile videoFile = this.videoServices.GetVideoFile(id);
            if (videoFile == null)
            {
                httpContext.Response.StatusCode = 404;
                return;
            }

            string header = httpContext.Request.Headers["Range"];
            long begin, end;

            if (this.TryRangeParse(header, videoFile, out begin, out end))
            {
                end = Math.Min(begin + maxTransfer, end);

                if (begin >= videoFile.Size || end > videoFile.Size)
                {
                    httpContext.Response.StatusCode = 416;
                    httpContext.Response.Headers.Add("Content-Range", $"bytes */{videoFile.Size}");
                    return;
                }

                httpContext.Response.StatusCode = 206;
                string rangeOut = $"bytes {begin}-{end}/{videoFile.Size}";
                httpContext.Response.ContentType = videoFile.ContentType;

                httpContext.Response.Headers.Add("Accept-Ranges", "bytes");
                httpContext.Response.Headers.Add("Content-Range", new Microsoft.Extensions.Primitives.StringValues(rangeOut));
                httpContext.Response.Headers.Add("Cache-Control", "no-cache");

                await videoFile.CopyTo(httpContext.Response.Body, begin, end);
            }
            else
            {
                httpContext.Response.ContentType = videoFile.ContentType;
                httpContext.Response.Headers.Add("Accept-Ranges", "bytes");
                httpContext.Response.Headers.Add("Cache-Control", "no-cache");

                await videoFile.CopyTo(httpContext.Response.Body);
            }
        }

        private bool TryRangeParse(string range, IVideoFile videoFile, out long begin, out long end)
        {
            const string bytesPrefix = "bytes=";
            begin = 0L;
            end = 0L;

            if (string.IsNullOrEmpty(range) || !range.StartsWith(bytesPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string rangeValues = range.Substring(bytesPrefix.Length);
            int delimiterIndex = rangeValues.IndexOf(&#39;-&#39;);

            if (!long.TryParse(rangeValues.Substring(0, delimiterIndex), out begin))
            {
                begin = 0L;
            }
            if (!long.TryParse(rangeValues.Substring(delimiterIndex + 1), out end))
            {
                end = videoFile.Size - 1;
            }

            return true;
        }
    }
```

## Prehrávanie videa
Video je možné jednoducho prehrať cez HTML5 tag _[video](http://www.w3schools.com/html/html5_video.asp)_, ktorý pomocou elementu _sourse_ dostane adresu videa.

Napríklad pre video s názvom _sample.mp4_ to bude _/videostream?file=sample.mp4_. Video je možné ľubovoľne prehrávať a posúvať.

```html
 <video id="mainPlayer" width="640" height="360"
     autoplay="autoplay" controls="controls" onloadeddata="onLoad()">
         <source src="@Model.VideoUrl" />
         <p>This user agents that do not support the video tag.</p>
 </video>
 <script type="text/javascript">
     //<![CDATA[
     function onLoad() {
         var sec = parseInt(document.location.search.substr(1));
         if (!isNaN(sec)) {
             mainPlayer.currentTime = sec;
         }
     }
     //]]>
 </script>
```

## Záver
Pri testovaní je potrebné v triede Startu ASP.NET MVC Core projektu pridať midllwer na video straming do pipline a zaregistrovať príslušnú službu. Pri praktickom nasadení je ešte potrebné zabrániť neoprávnenému sťahovaniu obsahu.

## Zdroje
1. [http://www.codeproject.com/Articles/820146/HTTP-Partial-Content-In-ASP-NET-Web-API-Video](http://www.codeproject.com/Articles/820146/HTTP-Partial-Content-In-ASP-NET-Web-API-Video)
1. [http://www.codeproject.com/Articles/813480/HTTP-Partial-Content-In-Node-js](http://www.codeproject.com/Articles/813480/HTTP-Partial-Content-In-Node-js)
1. [https://docs.asp.net/en/latest/fundamentals/middleware.html](https://docs.asp.net/en/latest/fundamentals/middleware.html)
1. [http://www.asp.net/aspnet/samples/owin-katana](http://www.asp.net/aspnet/samples/owin-katana)
1. [https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html](https://www.w3.org/Protocols/rfc2616/rfc2616-sec10.html)