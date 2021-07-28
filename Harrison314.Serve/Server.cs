using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.Serve
{
    internal class Server : IDisposable
    {
        private string path;
        private readonly ServerOptions serverOptions;
        private IWebHost webHost;

        public Server(string path, ServerOptions serverOptions)
        {
            this.path = path;
            this.serverOptions = serverOptions;
        }

        public void Dispose()
        {
            this.webHost?.Dispose();
        }

        public Task Start()
        {
            if (this.webHost == null)
            {
                this.webHost = this.BuildWebhost();
            }

            return this.webHost.StartAsync();
        }

        public Task Stop()
        {
            return this.webHost.StopAsync();
        }

        private IWebHost BuildWebhost()
        {
            IWebHost webHost = new WebHostBuilder()
                .UseContentRoot(this.path)
                .UseWebRoot(this.path)
                //.UseWebRoot(Path.Combine(this.path, "wwwroot"))
                .ConfigureLogging(loggingBuilder =>
                {
                    //if (loggerProviders is object)
                    //{
                    //    foreach (ILoggerProvider loggerProvider in loggerProviders)
                    //    {
                    //        if (loggerProvider is object)
                    //        {
                    //            loggingBuilder.AddProvider(
                    //                new ChangeLevelLoggerProvider(
                    //                    loggerProvider,
                    //                    level => level == LogLevel.Information ? LogLevel.Debug : level));
                    //        }
                    //    }
                    //}
                })
                .UseKestrel()
                .ConfigureKestrel(x => x.ListenAnyIP(this.serverOptions.Port))
                .ConfigureServices(this.ConfigureServices)
                .Configure(this.ConfigureApp)
                .Build();

            return webHost;
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            if (!string.IsNullOrEmpty(this.serverOptions.BasePath)
                && string.Equals(this.serverOptions.BasePath, "/", StringComparison.Ordinal))
            {
                app.UsePathBase(this.serverOptions.BasePath);
            }

            if (!string.IsNullOrEmpty(this.serverOptions.Http404Path))
            {
                app.UseStatusCodePagesWithRedirects(this.serverOptions.Http404Path);
            }

            if (this.serverOptions.LinkHideExtensions)
            {
                throw new NotSupportedException("Settings Keys.LinkHideExtensions = true is not supported in serve command");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
        }

        private void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
