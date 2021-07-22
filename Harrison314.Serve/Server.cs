using Microsoft.AspNetCore.Builder;
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
        private readonly int port;
        private IWebHost webHost;

        public Server(string path, int port)
        {
            this.path = path;
            this.port = port;
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
                .ConfigureKestrel(x => x.ListenAnyIP(this.port))
                .ConfigureServices(this.ConfigureServices)
                .Configure(this.ConfigureApp)
                .Build();

            return webHost;
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            app.UseStaticFiles();
            app.UseDefaultFiles();
        }

        private void ConfigureServices(IServiceCollection services)
        {

        }
    }
}
