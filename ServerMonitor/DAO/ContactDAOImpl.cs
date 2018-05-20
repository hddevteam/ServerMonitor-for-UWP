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

namespace ServerMonitor.DAO
{
    public class ContactDAOImpl : IContactDAO
    {
        #region Contact表操作
        /// <summary>
        /// 返回所有的联系人
        /// </summary>
        /// <returns></returns>
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
        /// 通过站点Id查询关联管理员
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<ContactModel> GetContactBySiteId(int siteId)
        {
            List<ContactModel> contactList;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                try
                {
                    contactList = conn.Table<ContactModel>().Where(v => v.SiteId == siteId).ToList<ContactModel>();
                }
                catch
                {
                    contactList = new List<ContactModel>();
                }
            }
            return contactList;
        }

        /// <summary>
        /// 插入一条联系人
        /// </summary>
        /// <param name="Contact"></param>
        /// <returns></returns>
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
        /// <param name="contactId"></param>
        /// <returns></returns>
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
        /// <param name="contact"></param>
        /// <returns></returns>
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
        //根据contactid输出contact信息
        private ContactModel GetContactByContactId(int ContactId)
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
        public List<ContactModel> GetContactModelsBySiteId(int siteid)
        {
            List<ContactModel> contactModels = new List<ContactModel>();
            IContactSiteDao k = ContactSiteDAOImpl.Instance;
            List<ContactSiteModel> contactSiteModels = k.GetConnectsBySiteId(siteid);
            foreach (ContactSiteModel m in contactSiteModels)
            {
                ContactModel tmp = GetContactByContactId(m.ContactId);
                contactModels.Add(tmp);
            }
            return contactModels;
        }
        private void InsertErrorLog(SQLiteException e)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
