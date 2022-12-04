using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minter_UI
{
    internal static class GlobalVar
    {
        static GlobalVar()
        {
            Settings.Initialize();
        }
        internal static bool CaseSensitiveFilehandling = true;
    }
}
