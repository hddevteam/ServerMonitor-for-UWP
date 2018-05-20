using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Controls;
using ServerMonitor.DAO;
using ServerMonitor.DAOImpl;
using ServerMonitor.Models;
using ServerMonitor.SiteDb;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TestServerMonitor.TestDAO
{
    [TestClass]
    public class GetContactModelsBySiteId
    {
        private int a;

        public object Count { get; private set; }
        [AssemblyInitialize]
        public static void Init(TestContext testContext)
        {
            DBInit db1 = DataBaseControlImpl.Instance;
            db1.InitDB("Filename.db");
        }
        [TestMethod]
        public void Method1()
        {
            List<ContactModel> l_list = new List<ContactModel>();
            IContactDAO n = new ContactDAOImpl();
            ContactModel l_CL = new ContactModel()
            {
                Id = 1,
                Contact_name = "song",
                Contact_email = "sing",
                Create_time = DateTime.Now,
                Update_time = DateTime.Now,
                Others = null,
                Telephone = null,
                SiteId = 1
            };
            n.InsertOneContact(l_CL);
            IContactSiteDao contactSiteDao = ContactSiteDAOImpl.Instance;
            contactSiteDao.InsertListConnects(new List<ContactSiteModel>()
            {
                new ContactSiteModel()
                {
                    SiteId=1,
                    ContactId=l_CL.Id
                }
            });
            l_list = n.GetContactModelsBySiteId(1);
            Assert.AreNotEqual(l_list.Count, 0);
            foreach (ContactModel m in l_list)
            {
                Assert.AreEqual(m.SiteId, 1);
            }
        }
    }
}
