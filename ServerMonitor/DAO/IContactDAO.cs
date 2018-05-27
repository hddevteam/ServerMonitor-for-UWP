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

        /// <summary>
        /// 根据SiteId寻找Contact
        /// </summary>
        /// <param name="siteid"></param>
        /// <returns>Contact列表</returns>
        List<ContactModel> GetContactModelsBySiteId(int siteid);
        /// <summary>
        /// 根据联系人ID查询对应的联系人
        /// </summary>
        ContactModel GetContactByContactId(int id);

        /// <summary>
        /// 插入异常抛出错误
        /// </summary>
        
    }
}
