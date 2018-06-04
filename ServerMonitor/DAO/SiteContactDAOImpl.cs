using ServerMonitor.DAO;
using ServerMonitor.Models;
using ServerMonitor.SiteDb;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.DAOImpl
{
    /// <summary>
    /// Author:xb
    /// </summary>
    public class SiteContactDAOImpl : ISiteContactDao
    {
        /// <summary>
        /// 延迟加载实例
        /// </summary>
        public static SiteContactDAOImpl Instance
        {
            get {
                return Nested.instance;
            }
        }

        /// <summary>
        /// 禁止直接生成实例
        /// </summary>
        private SiteContactDAOImpl() { }

        /// <summary>
        /// 根据单个站点ID删除此站点与指定ID联系人的记录
        /// </summary>
        /// <param name="SiteId">特定的站点ID</param>
        /// <param name="ConnectId">联系人ID</param>
        /// <returns>此操作影响的数据行数</returns>
        public int DeleteConnect(int SiteId, int ContactId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Execute("delete from Site_Contact where site_id = ? and contact_id = ?", SiteId, ContactId);
            }
            return result;
        }

        /// <summary>
        /// 删除指定ID的站点的全部绑定记录
        /// </summary>
        /// <param name="SiteId">指定站点的ID</param>
        /// <returns>此操作影响的数据行数</returns>
        public int DeletSiteAllConnect(int SiteId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Execute("delete from Site_Contact where site_id = ?", SiteId);
            }
            return result;
        }

        /// <summary>
        /// 通过指定的站点ID获取所有该站点的绑定记录
        /// </summary>
        /// <param name="SiteId">指定站点的ID</param>
        /// <returns>指定ID的站点的绑定记录的集合</returns>
        public List<SiteContactModel> GetConnectsBySiteId(int SiteId)
        {
            List<SiteContactModel> resultList;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                resultList = conn.Table<SiteContactModel>().Where(s=>s.SiteId == 
                SiteId).ToList<SiteContactModel>();
            }
            return resultList;
        }

        /// <summary>
        /// 插入一条绑定记录
        /// </summary>
        /// <param name="contact">待插入的绑定记录</param>
        /// <returns>此次操作影响的数据行数</returns>
        public int InsertConnect(SiteContactModel connect)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Insert(connect);
            }
            return result;
        }

        /// <summary>
        /// 插入一个绑定记录的集合
        /// </summary>
        /// <param name="connects">绑定记录集合</param>
        /// <returns>此次操作影响的数据行数</returns>
        public int InsertListConnects(List<SiteContactModel> connects)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.InsertAll(connects);
            }
            return result;
        }
        /// <summary>
        ///根据输入的SiteId从ContactSite表里查找ContactId
        /// </summary>
        /// <param name="siteid">输入的siteid</param>
        /// <returns></returns>
        public List<SiteContactModel> GetContactSiteBySiteId(int siteid)
        {

            List<SiteContactModel> contactsiteList;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                try
                {
                    contactsiteList = conn.Table<SiteContactModel>().Where(v => v.SiteId == siteid).ToList<SiteContactModel>();
                }
                catch
                {
                    contactsiteList = new List<SiteContactModel>();
                }
            }
            return contactsiteList;
        }
        /// <summary>
        /// 用于延迟加载的实例
        /// </summary>
        class Nested
        {
            static Nested()
            {

            }
            internal static readonly SiteContactDAOImpl instance = new SiteContactDAOImpl();
        }
    }
}
