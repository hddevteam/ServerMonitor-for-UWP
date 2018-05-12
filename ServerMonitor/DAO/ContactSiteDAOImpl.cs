using ServerMonitor.DAO;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.DAOImpl
{
    class Contact_SiteDAOImpl : IContact_SiteDao
    {
        public int DeleteConnect(int SiteId, int ConnectId)
        {
            throw new NotImplementedException();
        }

        public int DeletSiteAllConnect(int SiteId)
        {
            throw new NotImplementedException();
        }

        public List<ContactSiteModel> GetConnectInfoBySiteId(int SiteId)
        {
            throw new NotImplementedException();
        }

        public int InsertConnectInfo(ContactModel contact, SiteModel site)
        {
            throw new NotImplementedException();
        }
    }
}
