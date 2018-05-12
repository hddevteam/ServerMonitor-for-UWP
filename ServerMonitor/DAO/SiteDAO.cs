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
        /// <summary>
        /// 返回所有的站点
        /// </summary>
        /// <returns></returns>
        List<Site> GetAllSite();
        /// <summary>
        /// 根据站点id获取站点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Site GetSiteById(int id);
        /// <summary>
        /// 插入一个站点
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        int InsertOneSite(Site site);
        /// <summary>
        /// 插入多条站点
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        int InsertListSite(List<Site> sites);
        /// <summary>
        /// 删除一个站点
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        int DeleteOneSite(int siteId);
        /// <summary>
        /// 更新站点信息
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        int UpdateSite(Site site);
        /// <summary>
        /// 更新多个站点信息
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        int UpdateListSite(List<Site> sites);
        /// <summary>
        /// 执行有返回对象(Site)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        List<Site> DBExcuteSiteCommand(string command, object[] param);
    }
}
