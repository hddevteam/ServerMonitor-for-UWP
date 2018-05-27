using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.Controls;
using ServerMonitor.DAO;
using ServerMonitor.DAOImpl;
using ServerMonitor.Models;
using ServerMonitor.SiteDb;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
/// <summary>
/// 创建: mzy 创建时间: 2018/05/25
/// 联系人的相关操作
/// </summary>
namespace ServerMonitor.DAO
{
    /// <summary>
    /// 创建: mzy 创建时间: 2018/05/27 修改: mzy
    /// </summary>
    public class ContactDAOImpl : IContactDAO
    {
        #region Contact表操作
        /// <summary>
        /// 返回所有的联系人
        /// </summary>
        /// <returns>ContactModel列表</returns>
        public List<ContactModel> GetAllContact()
        {
            List<ContactModel> r;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                r = conn.Table<ContactModel>().ToList<ContactModel>();
            }
            return r;
        }


        /// <summary>
        /// 插入一条联系人
        /// </summary>
        /// <param name="Contact">联系人</param>
        /// <returns>成功返回插入条数</returns>
        public int InsertOneContact(ContactModel contact)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Insert(contact);
            }
            return result;
        }

        /// <summary>
        /// 删除一个联系人
        /// </summary>
        /// <param name="contactId">联系人ID</param>
        /// <returns>成功返回更改条数</returns>
        public int DeleteOneContact(int contactId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Execute("delete from Contact where Id = ?", contactId);
            }
            return result;
        }

        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="contact">联系人</param>
        /// <returns>更改条数</returns>
        public int UpdateContact(ContactModel contact)
        {
            // result = -1 表示异常返回值，执行操作失败
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                try
                {
                    result = conn.Update(contact);
                }
                // 若捕获到数据库相关的异常，如未找到此条记录
                catch (SQLite.Net.SQLiteException e)
                {
                    result = -1;
                    InsertErrorLog(e);
                }

            }
            return result;
        }

        /// <summary>
        /// 根据contactid输出contact信息
        /// </summary>    
        /// <param name="contactid">联系人ID</param>
        /// <returns>搜索到的联系人</returns>
        public ContactModel GetContactByContactId(int ContactId)
        {

            List<ContactModel> contactList;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                try
                {
                    contactList = conn.Table<ContactModel>().Where(v => v.Id == ContactId).ToList<ContactModel>();
                }
                catch
                {
                    contactList = new List<ContactModel>();
                }
            }
            return contactList[0];
        }

        /// <summary>
        /// 根据siteid输出contact信息
        /// </summary>    
        /// <param name="siteID">站点ID</param>
        /// <returns>ContaceModel列表</returns>
        public List<ContactModel> GetContactModelsBySiteId(int siteid)
        {
            List<ContactModel> contactModels = new List<ContactModel>();   //新建一个ContactModel表格用于储存获取到的信息
            IContactSiteDao ISCD = ContactSiteDAOImpl.Instance;    //调用IContactSiteDao里面的接口
            List<SiteContactModel> contactSiteModels = ISCD.GetConnectsBySiteId(siteid);
            //调用了ContactDAOImpl里面的GetConnectsBySiteId类，该类是通过输入的siteId从ContactSite表里的相对应的ContactId
            foreach (SiteContactModel m in contactSiteModels)//遍历所获得的contactSiteModels表里的信息，如果contactSiteModels里的ContactId与Contact表里的Id相对应，则输出Contact的信息，并加入 List<ContactModel> contactModels
            {
                ContactModel CM = GetContactByContactId(m.ContactId);
                contactModels.Add(CM);
            }
            return contactModels;
        }

        /// <summary>
        /// 插入失败抛出异常
        /// </summary>    
        public void InsertErrorLog(SQLiteException e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
