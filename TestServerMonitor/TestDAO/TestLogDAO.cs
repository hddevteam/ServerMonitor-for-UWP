using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.LogDb;
using ServerMonitor.SiteDb;
using ServerMonitor.ViewModels;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Popups;

namespace TestServerMonitor.TestDAO
{
    [TestClass]
    public class TestLogDAO
    {
        /// <summary>
        /// 测试用log列表
        /// </summary>
        List<LogModel> logModels = new List<LogModel>();
        /// <summary>
        /// 测试用logDAO
        /// </summary>
        LogDAO logDAO = new LogDaoImpl();
        /// <summary>
        /// 测试用log
        /// </summary>
        LogModel log;

        [TestMethod]
        [AssemblyInitialize]
        public static void InitDatabase(TestContext testContext)
        {
            DBInit db = DbInitImpl.Instance;
            db.InitDB("testdb.db");
        }
        [TestMethod]
        [TestInitialize]
        [Priority(1)]
        public void Init()//初始化数据库并向log列表填充数据
        {
            logModels = new List<LogModel>();
            LogModel logModel1 = new LogModel()
            {
                //Id = 66,
                Site_id = 88,
                Status_code = "200",
                Request_time = 200,
                Create_time = DateTime.Now,
                Is_error = false,
                Log_record = ""
            };
            LogModel logModel2 = new LogModel()
            {
                //Id = 67,
                Site_id = 88,
                Status_code = "200",
                Request_time = 200,
                Create_time = DateTime.Now,
                Is_error = true,
                Log_record = ""
            };
            logModels.Add(logModel1);
            logModels.Add(logModel2);
            log = new LogModel()
            {
                //Id = 69,
                Site_id = 89,
                Status_code = "200",
                Request_time = 200,
                Create_time = DateTime.Now,
                Is_error = false,
                Log_record = ""
            };
        }
        ///<summary>
        ///测试InsertOneLog
        ///</summary>
        [TestMethod]
        [Priority(1)]
        public void TestLogDAO_InsertOneLog()
        {
            Assert.AreEqual(1,logDAO.InsertOneLog(log));//插入log
            Assert.AreEqual(log.Id, logDAO.GetLogById(log.Id).Id);//log插入后获得id，然后看取到值的id，是否和log的id相等
        }
        /// <summary>
        /// 测试插入一条空的log
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void TestLogDAO_InsertOneLog_Isnull()
        {
            LogModel testlog = null;
            Assert.AreEqual(0, logDAO.InsertOneLog(testlog));//插入一条为null的log，看返回值是否为0

        }
        /// <summary>
        /// 测试InsertListLog
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void TestLogDAO_InsertListLog()
        {
            Assert.AreEqual(logModels.Count, logDAO.InsertListLog(logModels));
            Assert.AreNotEqual(0, logDAO.GetLogsBySiteId(logModels[0].Site_id).Count);//按照插入的log的site_id进行查询看查到的是否为空的List<LogModel>

        }
        /// <summary>
        /// 测试GetAllLog
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetAllLog()
        {

            Assert.AreNotEqual(0, logDAO.GetAllLog().Count);//如果等于0，查出一个空的List<LogModel>
        }
        /// <summary>
        /// 测试GetLogById
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetLogById()
        {
            Assert.AreNotEqual(0, logDAO.GetLogById(logDAO.GetAllLog()[0].Id));
            
        }
        /// <summary>
        /// 用预先插入的Log的Site_id进行测试GetLogBySiteId
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetLogBySiteId()
        {
            LogModel testlog = new LogModel()
            {
                Site_id = 888,
                Status_code = "200",
                Request_time = 200,
                Create_time = DateTime.Now,
                Is_error = false,
                Log_record = ""
            };
            logDAO.InsertOneLog(testlog);
            Assert.AreEqual(testlog.Id, logDAO.GetLogsBySiteId(testlog.Site_id)[0].Id);
        }
        /// <summary>
        /// 测试DeleteOneLog
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DeleteOneLog()
        {
            //添加测试Log,目的通过这个log的插入和删除测试删除操作
            LogModel testlog = new LogModel()
            {
                Site_id = 666,
                Status_code = "200",
                Request_time = 200,
                Create_time = DateTime.Now,
                Is_error = false,
                Log_record = ""
            };
            logDAO.InsertOneLog(testlog);
            Assert.AreEqual(1, logDAO.DeleteOneLog(testlog.Id));
            Assert.AreEqual(0, logDAO.GetLogById(testlog.Id).Site_id);//取一下testlog，看是否取到空log
        }
        /// <summary>
        /// 用预先插入的Log的Site_id进行测试DeleteLogsBySite
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DeleteLogsBySite()
        {
            

            Assert.AreEqual(2, logDAO.DeleteLogsBySite(logModels[0].Site_id));
            Assert.AreEqual(0,logDAO.GetLogById(logModels[0].Site_id).Site_id);//取一下logmodel[0]，看取得的是否为空log

            
        }
        /// <summary>
        /// 测试UpdateLog
        /// </summary>
        [TestMethod]
        [Priority(3)]
        public void TestLogDAO_UpdateLog()
        {
            LogModel testlog = new LogModel()
            {
                Site_id = 666,
                Status_code = "200",
                Request_time = 200,
                Create_time = DateTime.Now,
                Is_error = false,
                Log_record = ""
            };
            logDAO.InsertOneLog(testlog);//将testlog插入
            testlog.Is_error = true;//改变testlog的Is_error为true
            logDAO.UpdateLog(testlog);
            Assert.AreEqual(true, logDAO.GetLogById(testlog.Id).Is_error);//对比updatelog后testlog的Is_error是否改变
        }
        /// <summary>
        /// 测试UpdateListLog
        /// </summary>
        [TestMethod]
        [Priority(3)]
        public void TestLogDAO_UpdateListLog()
        {
            logDAO.InsertListLog(logModels);
            logModels[0].Log_record = "ok";//将插入的logmodel[0]的Log_record字段改为ok
            logDAO.UpdateListLog(logModels);
            Assert.AreEqual("ok", logDAO.GetLogById(logModels[0].Id).Log_record);//对比Log_record字段是否发生改变

        }
        /// <summary>
        /// 测试InertErrorLog
        /// </summary>
        [TestMethod]
        public void TestLogDAO_InsertErrorLog()
        {
            
            try
            {
                throw new Exception();//主动抛出异常
            }
            // 捕获异常
            catch (Exception e)
            {
                
                Assert.AreEqual(1, logDAO.InsertErrorLog(e));
            }
            ;
        }
        /// <summary>
        /// 测试DBExcuteLogCommand
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DBExcuteLogCommand()
        {
            object[] o = new object[] { };
            Assert.AreNotEqual(null, logDAO.DBExcuteLogCommand("select *from Log", o));
        }

    }
}
