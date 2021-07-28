using Statiq.App;
using Statiq.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.Serve
{
    public static class BootstrapperCommandExtensions
    {
        public static Bootstrapper AddServeCommand(this Bootstrapper bootstrapper, Action<ServeCommandOptions> options = null)
        {
            ServeCommandOptions serveCommandOptions = new ServeCommandOptions();
            options?.Invoke(serveCommandOptions);
            bootstrapper.AddSetting(ServeKeys.Http404Path, serveCommandOptions.Http404Path);

            return bootstrapper.AddCommand<ServeCommand>();
        }
    }
}
