using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.SiteDb
{
    public interface SiteDAO
    {
        List<Site> GetAllSite();
        Site GetSiteById(int id);
        int InsertOneSite(Site site);
        int InsertListSite(List<Site> sites);
        int DeleteOneSite(int siteId);
        int UpdateSite(Site site);
        int UpdateListSite(List<Site> sites);
        /// <summary>
        /// 使用SQL语句对Site数据库进行修改
        /// </summary>
        /// <param name="command">sql语句</param>
        /// <param name="param">参数</param>
        /// <returns></returns>
        List<Site> DBExcuteSiteCommand(string command, object[] param);
    }
}
