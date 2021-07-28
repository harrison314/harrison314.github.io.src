using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.App;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Harrison314.Serve
{
    // See: https://github.com/duracellko/Statiq.Framework/tree/serve/src/core/Statiq.Hosting

    [Description("Simple serve command.")]
    public class ServeCommand : InteractiveCommand<ServeCommandSettings>
    {
        private FileSystemWatcher inputFileWatcher;
        private Server server;

        public ServeCommand(IConfiguratorCollection configurators, Settings settings, IServiceCollection serviceCollection, Bootstrapper bootstrapper) : base(configurators, settings, serviceCollection, bootstrapper)
        {
            this.inputFileWatcher = null;
        }

        protected override async Task AfterInitialExecutionAsync(CommandContext commandContext, ServeCommandSettings commandSettings, IEngineManager engineManager, CancellationTokenSource cancellationTokenSource)
        {
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            IDirectory outputDirectory = engineManager.Engine.FileSystem.GetOutputDirectory();
            var inputDirectorys = engineManager.Engine.FileSystem.GetInputDirectories();
            var inputDirectory = inputDirectorys.Where(t => t.Exists).First();


            this.inputFileWatcher = new FileSystemWatcher(inputDirectory.Path.FullPath);

            this.inputFileWatcher.IncludeSubdirectories = true;
            this.inputFileWatcher.Changed += this.InputFileWatcher_Changed;
            this.inputFileWatcher.Created += this.InputFileWatcher_Changed;
            this.inputFileWatcher.Renamed += this.InputFileWatcher_Changed;

            this.inputFileWatcher.EnableRaisingEvents = true;

            ServerOptions serverOptions = new ServerOptions()
            {
                Http404Path = engineManager.Engine.Settings.GetString(ServeKeys.Http404Path, null),
                BasePath = engineManager.Engine.Settings.GetString(Keys.LinkRoot, null),
                Port = commandSettings.Port,
                LinkHideExtensions = engineManager.Engine.Settings.GetBool(Keys.LinkHideExtensions, false)
            };

            this.server = new Server(outputDirectory.Path.FullPath, serverOptions);

            await this.server.Start();
            logger.LogInformation("Start server on {0}", $"http://localhost:{commandSettings.Port}/ ");
        }

        protected override async Task ExitingAsync(CommandContext commandContext, ServeCommandSettings commandSettings, IEngineManager engineManager)
        {
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();

            this.inputFileWatcher?.Dispose();

            if (this.server != null)
            {
                await this.server.Stop();
                this.server.Dispose();
            }
        }

        private void InputFileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.TriggerExecution();
        }
    }
}
