using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.DAO;
using ServerMonitor.SiteDb;
using ServerMonitor.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using Microsoft.VisualStudio.TestTools.UnitTesting;
/// <summary>
/// 创建: mzy
/// 测试ContactDAOImpl文件的相关方法
/// </summary>
namespace TestServerMonitor.TestDAO
{
    [TestClass]
    public class TestContactDAO
    {
        //实例化测试对象
        private ContactDAOImpl contactDAO = new ContactDAOImpl();
        //ContactModel对象列表,用于测试方法
        private List<ContactModel> lc;
        //定义一条SiteContactModel类型数据
        private SiteContactModel sm;

        [ClassInitialize]
        [Priority(1)]
        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="testContext"></param>
        //创建测试用数据库test.sqlite
        public static void InitDatabase(TestContext testContext)
        {
            DBInit db = DataBaseControlImpl.Instance;
            db.InitDB("test.sqlite");
        }

        [TestInitialize]
        [Priority(1)]
        /// <summary>
        /// 测试之前初始化, 初始化3条ContactModel数据
        /// </summary>
        public void Init()
        {
            //初始化SiteContactModel数据项
            sm = new SiteContactModel() { ContactId = 1, SiteId = 1 };

            //暂不插入数据库,测试时使用
            lc = new List<ContactModel>
            {
                new ContactModel
                {
                    Contact_name = "Tom",
                    Contact_email = "1@qq.com"                 
                },

                new ContactModel
                {
                    Contact_name = "Rain",
                    Contact_email = "2@qq.com"                   
                },

                new ContactModel
                {
                    Contact_name = "Jerry",
                    Contact_email = "3@qq.com"            
                }
            };
        }

        /// <summary>
        /// 测试InsertOneContact方法
        /// 用例说明：对比插入方法的返回值是否和1相等判断是否插入成功
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestInsertOneContact()
        {
            int result = -1;
            result = contactDAO.InsertOneContact(lc[0]);

            Assert.AreEqual(1, result);
        }


        /// <summary>
        /// 测试DeleteOneContact方法
        /// 用例说明：删除成功result为1
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestDeleteOneContact()
        {
            int result = -1;
            int contactId = 2;//默认数据表自增ID从1开始
            contactDAO.InsertOneContact(lc[1]);//先插入lc[1], 再删除
            result = contactDAO.DeleteOneContact(contactId);

            Assert.AreEqual(1, result);
        }

        /// <summary>
        /// 测试UpdateContact方法
        /// 用例说明：插入一条数据后更改, 再从数据库中取出并且判断是否更改, 最后删除数据
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestUpdateContact()
        {
            int result = -1;

            contactDAO.InsertOneContact(lc[2]);//将一条数据插入
            lc[2].Contact_email = "333@qq.com";//更改上次插入的email 
            result = contactDAO.UpdateContact(lc[2]);
            Assert.AreEqual(1, result);
            ContactModel contact = contactDAO.GetContactByContactId(4);
            Assert.AreEqual(contact.Contact_email, "333@qq.com");//判断是否更改成功

            contactDAO.DeleteOneContact(4);//删除记录
        }

        /// <summary>
        /// 测试GetContactByContactId方法
        /// 用例说明: 根据ContactId获得一条Contact数据项, 根据返回值判断是否相等
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestGetContactByContactId()
        {
            int id = 1;
            ContactModel oneContact = contactDAO.GetContactByContactId(id);
            Assert.AreEqual(oneContact.Contact_name, "Tom");//根据返回值判断是否与lc[0]相等
        }

        /// <summary>
        /// 测试GetAllContact方法
        /// 用例说明:在Insert方法执行后, 数据表中有一条数据, 根据GetAllContact()方法返回数据的个数判断是否执行成功
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestGetAllContact()
        {
            //判断GetAllContact()函数的返回列表中记录的条数是否与实际相等
            Assert.AreEqual(1, contactDAO.GetAllContact().Count());//数据表中只有一条记录
        }

        /// <summary>
        /// 测试GetContactModelsBySiteId方法
        /// 用例说明: 先向site_contact表中插入一条数据, 从site_contact取得SiteId为1的第一条数据与lc[0]进行对比 
        /// </summary>
        [TestMethod]
        [Priority(3)]
        public void TestGetContactModelsBySiteId()
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Insert(sm);
            }
            ContactModel contact = contactDAO.GetContactModelsBySiteId(1)[0];
            Assert.AreEqual(contact.Contact_name, "Tom");
        }
    }
}
