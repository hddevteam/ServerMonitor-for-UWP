using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.Models;
using ServerMonitor.DAO;

namespace ServerMonitor.DAO
{
    public interface IContactDAO
    {
        /// <summary>
        /// 返回所有的联系人
        /// </summary>
        /// <returns></returns>
        List<ContactModel> GetAllContact();

        /// <summary>
        /// 通过站点Id查询关联管理员
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        List<ContactModel> GetContactBySiteId(int siteId);

        /// <summary>
        /// 插入一条联系人
        /// </summary>
        /// <param name="Contact"></param>
        /// <returns></returns>
        int InsertOneContact(ContactModel contact);

        /// <summary>
        /// 删除一个联系人
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        int DeleteOneContact(int contactId);

        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        int UpdateContact(ContactModel contact);

        List<ContactModel> GetContactModelsBySiteId(int siteid);
    }
}
