using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.Controls;
using ServerMonitor.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace ServerMonitor.SiteDb
{
    
    class SiteDaoImpl : SiteDAO
    {
        /// <summary>
        /// 执行有返回对象(Site)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<Site> DBExcuteSiteCommand(string command, object[] param)
        {
            List<Site> result;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                SQLiteCommand cmd = conn.CreateCommand(command, param);
                result = cmd.ExecuteQuery<Site>();
            }
            return result;
        }
        /// <summary>
        /// 更新站点信息
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public int UpdateSite(Site site)
        {
            // result = -1 表示异常返回值，执行操作失败
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                try
                {
                    result = conn.Update(site);
                    Debug.WriteLine(string.Format("更新站点{0} 成功！站点当前已经请求{1}次", site.Id, site.Request_count));
                }
                // 若捕获到数据库相关的异常，如未找到此条记录
                catch (SQLite.Net.SQLiteException e)
                {
                    result = -1;
                    DBHelper.InsertErrorLog(e);
                }

            }
            return result;
        }
        /// <summary>
        /// 删除一个站点
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public int DeleteOneSite(int siteId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.Execute("delete from Site where Id = ?", siteId);
            }
            return result;
        }
        /// <summary>
        /// 返回所有的站点
        /// </summary>
        /// <returns></returns>
        public List<Site> GetAllSite()
        {
            List<Site> r;//返回Site列表
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                r = conn.Table<Site>().ToList<Site>();
            }
            return r;
        }
        /// <summary>
        /// 根据站点id获取站点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Site GetSiteById(int id)
        {
            Site s;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                try
                {
                    s = conn.Table<Site>().Where(v => v.Id == id).ToList<Site>()[0];
                }
                catch
                {
                    s = new Site();
                }
            }
            return s;
        }
        /// <summary>
        /// 插入多条站点
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public int InsertListSite(List<Site> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.InsertAll(sites);
            }
            return result;
        }
        /// <summary>
        /// 插入一个站点
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public int InsertOneSite(Site site)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.Insert(site);
            }
            return result;
        }
        /// <summary>
        /// 更新多个站点信息
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public int UpdateListSite(List<Site> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.UpdateAll(sites);
            }
            return result;
        }

       
    }
}
