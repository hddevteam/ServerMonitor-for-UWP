using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.DAO
{
    /// <summary>
    /// Author:xb
    /// </summary>
    public interface IContactSiteDao
    {        
        /// <summary>
        /// 插入一条绑定记录
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        int InsertConnect(ContactModel contact);    
        /// <summary>
        /// 根据站点的id获取绑定记录
        /// </summary>
        /// <param name="SiteId"></param>
        /// <returns></returns>
        List<ContactSiteModel> GetConnectsBySiteId(int SiteId);
        /// <summary>
        /// 根据单个站点ID删除此站点与指定ID联系人的记录
        /// </summary>
        /// <param name="SiteId">特定的站点ID</param>
        /// <param name="ConnectId">联系人ID</param>
        /// <returns>此操作影响的数据行数</returns>
        int DeleteConnect(int SiteId, int ContactId);
        /// <summary>
        /// 删除指定ID的站点的全部绑定记录
        /// </summary>
        /// <param name="SiteId">指定站点的ID</param>
        /// <returns>此操作影响的数据行数</returns>
        int DeletSiteAllConnect(int SiteId);
        /// <summary>
        /// 插入一系列绑定记录
        /// </summary>
        /// <param name="connects"></param>
        /// <returns></returns>
        int InsertListConnects(List<ContactSiteModel> connects);
        List<ContactSiteModel> GetContactSiteBySiteId(int siteid);
    }
}
