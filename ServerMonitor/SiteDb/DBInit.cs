using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.SiteDb
{
    interface DBInit
    {
        void SetDBFilename(string Filename);
        void InitDB(string DBFilename);
    }
}
