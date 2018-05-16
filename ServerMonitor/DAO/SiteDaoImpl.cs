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
        public List<SiteModel> DBExcuteSiteCommand(string command, object[] param)
        {
            List<SiteModel> result;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                SQLiteCommand cmd = conn.CreateCommand(command, param);
                result = cmd.ExecuteQuery<SiteModel>();
            }
            return result;
        }
        /// <summary>
        /// 更新站点信息
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public int UpdateSite(SiteModel site)
        {
            // result = -1 表示异常返回值，执行操作失败
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
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
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Execute("delete from Site where Id = ?", siteId);
            }
            return result;
        }
        /// <summary>
        /// 返回所有的站点
        /// </summary>
        /// <returns></returns>
        public List<SiteModel> GetAllSite()
        {
            List<SiteModel> r;//返回Site列表
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                r = conn.Table<SiteModel>().ToList<SiteModel>();
            }
            return r;
        }
        /// <summary>
        /// 根据站点id获取站点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SiteModel GetSiteById(int id)
        {
            SiteModel s;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                try
                {
                    s = conn.Table<SiteModel>().Where(v => v.Id == id).ToList<SiteModel>()[0];
                }
                catch
                {
                    s = new SiteModel();
                }
            }
            return s;
        }
        /// <summary>
        /// 插入多条站点
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public int InsertListSite(List<SiteModel> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
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
        public int InsertOneSite(SiteModel site)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
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
        public int UpdateListSite(List<SiteModel> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.UpdateAll(sites);
            }
            return result;
        }

       
    }
}
