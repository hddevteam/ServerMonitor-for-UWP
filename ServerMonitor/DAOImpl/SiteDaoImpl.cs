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
        /// 使用SQL语句对Site数据库进行修改
        /// </summary>
        /// <param name="command">sql语句</param>
        /// <param name="param">sql语句所需参数</param>
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
        public int DeleteOneSite(int siteId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.Execute("delete from Site where Id = ?", siteId);
            }
            return result;
        }

        public List<Site> GetAllSite()
        {
            List<Site> r;//返回Site列表
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                r = conn.Table<Site>().ToList<Site>();
            }
            return r;
        }

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

        public int InsertListSite(List<Site> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.InsertAll(sites);
            }
            return result;
        }

        public int InsertOneSite(Site site)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DbInitImpl.DBPath1))
            {
                result = conn.Insert(site);
            }
            return result;
        }

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
