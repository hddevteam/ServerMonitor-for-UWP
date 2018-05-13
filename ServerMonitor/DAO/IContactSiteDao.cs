using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.DAO
{
    interface IContactSiteDao
    {
        
        int InsertConnect(ContactModel contact);
        
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
        int InsertListConnects(List<ContactSiteModel> connects);
    }
}
