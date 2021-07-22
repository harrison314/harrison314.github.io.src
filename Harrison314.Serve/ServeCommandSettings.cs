using Spectre.Console.Cli;
using Statiq.App;
using System.ComponentModel;

namespace Harrison314.Serve
{
    public class ServeCommandSettings : PipelinesCommandSettings
    {
        [CommandOption("--port <PORT>")]
        [Description("Start serve server on port. (default is 5080).")]
        public int Port 
        {
            get;
            set; 
        }

        public ServeCommandSettings()
        {
            this.Port = 5080;
        }
    }
}
