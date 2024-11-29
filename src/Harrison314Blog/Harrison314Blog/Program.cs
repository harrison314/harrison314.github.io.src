using AspNetStatic;
using AspNetStaticContrib.AspNetStatic;
using Harrison314Blog.Components;
using Microsoft.Extensions.FileProviders;

namespace Harrison314Blog;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IUrlManager>(new UrlManager("https://harrison314.github.io/"));
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.AddSingleton<IStaticResourcesInfoProvider>(
            new StaticResourcesInfoProvider()
            .AddContent()
            .AddMarkdawns(out IPostDictionary postDictionary)
            .AddAllWebRootContent(builder.Environment)); 
        builder.Services.AddSingleton<IPostDictionary>(postDictionary);

        WebApplication app = builder.Build();

        //if (!app.Environment.IsDevelopment())
        //{
        //    app.UseExceptionHandler("/Error");
        //}

        string outPath = Path.GetFullPath(@"./bin/data");
        if (args.Contains("--preview"))
        {
            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                RequestPath = "",
                FileProvider = new PhysicalFileProvider(outPath),
                RedirectToAppendTrailingSlash = true,
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "",
                FileProvider = new PhysicalFileProvider(outPath)
            });
        }
        else
        {
            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseAntiforgery();

            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            FeedEndpoint.RegisterRss(app);
            FeedEndpoint.RegisterSitemap(app);

          
            bool exitingArg = args.HasExitWhenDoneArg();
            Directory.CreateDirectory(outPath);
            app.GenerateStaticContent(outPath, exitWhenDone: exitingArg);
        }
        
        app.Run();
    }
}
