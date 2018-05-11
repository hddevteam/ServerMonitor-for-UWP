using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.SiteDb
{
    public interface SiteDao
    {
        List<Site> GetAllSite();
        Site GetSiteById(int id);
        int InsertOneSite(Site site);
        int InsertListSite(List<Site> sites);
        int DeleteOneSite(int siteId);
        int UpdateSite(Site site);
        int UpdateListSite(List<Site> sites);
        List<Site> DBExcuteSiteCommand(string command, object[] param);
    }
}
