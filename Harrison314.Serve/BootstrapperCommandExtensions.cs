using Statiq.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.Serve
{
    public static class BootstrapperCommandExtensions
    {
        public static Bootstrapper AddServeCommand(this Bootstrapper bootstrapper)
        {
            return bootstrapper.AddCommand<ServeCommand>();
        }
    }
}
