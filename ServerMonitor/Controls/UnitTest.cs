using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Controls
{
    class UnitTest
    {
        public UnitTest() {
            DBHelper.InitDB("ServerMonitor.sqlite");
            Log t1 = new Log() { Log_record = "测试1" };
            Site t2 = new Site() { Site_name = "Google" };
            int a1 = DBHelper.InsertListLog(new List<Log>() { t1 });
            int a2 = DBHelper.InsertListSite(new List<Site>() { t2 });
            int a3 = DBHelper.InsertOneLog(t1);
            int a4 = DBHelper.InsertOneSite(t2);
            Log t3 = DBHelper.GetLogById(1);
            Site t4 = DBHelper.GetSiteById(1);
            List<Log> l_all = DBHelper.GetAllLog();
            List<Site> s_all = DBHelper.GetAllSite();            
        }
    }
}
