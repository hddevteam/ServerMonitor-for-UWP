using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.DAO
{
    interface IContact_SiteDao
    {
        int InsertConnectInfo(ContactModel contact, SiteModel site);
        List<ContactSiteModel> GetConnectInfoBySiteId(int SiteId);
        int DeleteConnect(int SiteId, int ConnectId);
        int DeletSiteAllConnect(int SiteId);
    }
}
