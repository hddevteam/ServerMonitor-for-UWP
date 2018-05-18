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
        List<SiteModel> GetAllSite();
        /// <summary>
        /// 根据站点id获取站点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        SiteModel GetSiteById(int id);
        /// <summary>
        /// 插入一个站点
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        int InsertOneSite(SiteModel site);
        /// <summary>
        /// 插入多条站点
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        int InsertListSite(List<SiteModel> sites);
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
        int UpdateSite(SiteModel site);
        /// <summary>
        /// 更新多个站点信息
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        int UpdateListSite(List<SiteModel> sites);
        /// <summary>
        /// 执行有返回对象(Site)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        List<SiteModel> DBExcuteSiteCommand(string command, object[] param);
        int SetAllSiteStatus(int statusCode);
    }
}
