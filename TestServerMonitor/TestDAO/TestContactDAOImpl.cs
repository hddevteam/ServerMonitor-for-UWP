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
    public class TestContactDAOImpl
    {
        private int a;

        public object Count { get; private set; }
        [ClassInitialize] // 测试类生成预处理
        public static void Init(TestContext testContext)
        {
            DBInit db1 = DataBaseControlImpl.Instance;
            db1.InitDB("Filename.db");
        }//对数据库进行初始化，连接数据库
        [TestMethod]
        public void GetContactModelsBySiteId()
        {
            List<ContactModel> l_list = new List<ContactModel>();
            //新建一个表，验证表
            IContactDAO ICD = new ContactDAOImpl();//调用接口
            ContactModel l_CL = new ContactModel()
            {
                Id = 1,
                Contact_name = "song",
                Contact_email = "sing",
                Create_time = DateTime.Now,
                Update_time = DateTime.Now,
                Others = null,
                Telephone = null,
            };
            ICD.InsertOneContact(l_CL);
            //为了防止数据库起初没数据，为表插入一条Contact信息，使测试通过
            IContactSiteDao contactSiteDao = ContactSiteDAOImpl.Instance;
            contactSiteDao.InsertListConnects(new List<SiteContactModel>()
            {
                new SiteContactModel()
                {
                    SiteId=1,
                    ContactId=l_CL.Id
                    //注意Contact里面的Id是自动生成，不一定是输入的值，所以ContactSite的Id需要获取一下当前Contact的Id
                }
            });
            //插入一条ContactSite信息
            l_list = ICD.GetContactModelsBySiteId(1);
            Assert.AreNotEqual(l_list.Count, 0);
            //已经有数据看结果是否有值，验证方法
        }
    }
}
