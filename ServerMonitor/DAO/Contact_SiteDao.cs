using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.DAO
{
    interface Contact_SiteDao
    {
        int InsertConnectInfo(Contact contact, Site site);
        List<Contact_Site> GetConnectInfoBySiteId(int SiteId);
        int DeleteConnect(int SiteId, int ConnectId);
        int DeletSiteAllConnect(int SiteId);
    }
}
