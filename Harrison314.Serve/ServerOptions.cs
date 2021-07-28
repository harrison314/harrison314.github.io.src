using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harrison314.Serve
{
    internal class ServerOptions
    {
        public int Port
        {
            get;
            set;
        }

        public string Http404Path
        {
            get;
            set;
        }

        public string BasePath
        {
            get;
            set;
        }

        public bool LinkHideExtensions
        {
            get;
            set;
        }

        public ServerOptions()
        {

        }
    }
}
