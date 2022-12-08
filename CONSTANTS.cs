using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scalax_admin.CONSTANTS
{
    internal class CONSTANTS
    {
        private static readonly bool isDevEnv = false;

        public static readonly string SERVER_ENDPOINT_URL = isDevEnv ? "http://127.0.0.1:5000" : "https://site--scalax-server-01--n5gfpf86nbvs.code.run";
    }
}
