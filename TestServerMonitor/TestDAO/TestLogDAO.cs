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
/// <summary>
/// 创建：zhanglin  创建时间：2018/05/27
/// 测试LogDAO
/// </summary>
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
        /// 测试用ILogDAO的实例
        /// </summary>
        ILogDAO logDAO = new LogDaoImpl();
        /// <summary>
        /// 测试用log
        /// </summary>
        LogModel log;
        /// <summary>
        /// 初始化一个测试用的数据库，命名为testdb.db
        /// </summary>
        /// <param name="testContext"></param>
        [ClassInitialize]
        public static void InitDatabase(TestContext testContext)
        {
            DBInit db = DataBaseControlImpl.Instance;
            db.InitDB("testdb.db");
        }
        /// <summary>
        /// 为测试用log和logModels赋值
        /// </summary>
        [TestMethod]
        [TestInitialize]
        [Priority(1)]
        public void Init()
        {
            logModels = new List<LogModel>();
            LogModel logModel1 = new LogModel()
            {
                Site_id = 88,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = false,
                Log_Record = ""
            };
            LogModel logModel2 = new LogModel()
            {
                Site_id = 88,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = true,
                Log_Record = ""
            };
            logModels.Add(logModel1);
            logModels.Add(logModel2);
            log = new LogModel()
            {
                Site_id = 89,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = false,
                Log_Record = ""
            };
        }
        ///<summary>
        ///测试插入一条Log
        ///说明：插入一条Log看Return值是否为1，然后再根据id查找，看是否能查到
        ///</summary>
        [TestMethod]
        [Priority(1)]
        public void TestLogDAO_InsertOneLog()
        {
            Assert.AreEqual(1,logDAO.InsertOneLog(log));//插入log
            Assert.AreEqual(log.Id, logDAO.GetLogById(log.Id).Id);//log插入后获得id，然后看取到值的id，是否和log的id相等
            logDAO.DeleteOneLog(log.Id);//清除插入数据
        }
        /// <summary>
        /// 测试插入一条空的log
        /// 说明：插入一条为空的Log，看返回值是否为0
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void TestLogDAO_InsertOneLog_Isnull()
        {
            LogModel testlog = null;
            Assert.AreEqual(0, logDAO.InsertOneLog(testlog));//插入一条为null的log，看返回值是否为0

        }
        /// <summary>
        /// 测试插入一个Log列表
        /// 说明：插入logModels，看返回值是否为列表元素个数，然后再根据site_id进行查询看是否为空
        /// </summary>
        [TestMethod]
        [Priority(1)]
        public void TestLogDAO_InsertListLog()
        {
            Assert.AreEqual(logModels.Count, logDAO.InsertListLog(logModels));
            Assert.AreEqual(2, logDAO.GetLogsBySiteId(logModels[0].Site_id).Count);//按照插入的log的site_id进行查询看是否查到
            logDAO.DeleteLogsBySite(88);//清除插入数据

        }
        /// <summary>
        /// 测试获取所有的Log
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetAllLog()
        {

            logDAO.InsertListLog(logModels);
            Assert.AreEqual(2, logDAO.GetAllLog().Count);//如果等于0，查出一个空的List<LogModel>
            Assert.AreEqual(logModels[0].Id, logDAO.GetAllLog()[0].Id);
            logDAO.DeleteLogsBySite(88);//清除插入数据
        }
        /// <summary>
        /// 测试根据Id查询Log
        /// 说明：将GetAllLog查询到的Log列表的的第一个元素的Id进行查询，看是否查询得到
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetLogById()
        {
            logDAO.InsertListLog(logModels);
            Assert.AreEqual(logModels[0].Id, logDAO.GetLogById(logModels[0].Id).Id);
            logDAO.DeleteLogsBySite(88);//清除插入数据

        }
        /// <summary>
        /// 测试根据不存在的Id查询Log
        /// 说明：查询不存在id
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetLogById_Error_Id()
        {
            logDAO.InsertListLog(logModels);
            Assert.AreEqual(0, logDAO.GetLogById(55).Id);//查找一个不存在的id
            logDAO.DeleteLogsBySite(88);//清除插入数据

        }
        /// <summary>
        /// 用预先插入的Log的Site_id进行测试GetLogBySiteId
        /// 说明：插入一条testlog，然后根据testlog的site_id进行查询，看查到的Log是否为testlog
        /// </summary>
        [TestMethod]
        [Priority(2)]
        public void TestLogDAO_GetLogBySiteId()
        {
            LogModel testlog = new LogModel()
            {
                Site_id = 888,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = false,
                Log_Record = ""
            };
            logDAO.InsertOneLog(testlog);
            Assert.AreEqual(testlog.Site_id, logDAO.GetLogsBySiteId(testlog.Site_id)[0].Site_id);
            logDAO.DeleteOneLog(testlog.Id);//清除插入数据
        }
        /// <summary>
        /// 测试查找一个不存在的site_id
        /// </summary>
        [TestMethod]
        public void TestLogDAO_GetLogBySiteId_Error_site_id()
        {
            logDAO.InsertListLog(logModels);
            Assert.AreEqual(0, logDAO.GetLogsBySiteId(55).Count);//查找Error_site_id
            logDAO.DeleteLogsBySite(88);//清除插入数据
        }
        /// <summary>
        /// 测试删除一条Log
        /// 说明：先插入一条testlog，然后将其删除看返回值是否为1，然后在根据testlog的id进行查询看查到的Log是否为空Log
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DeleteOneLog()
        {
            //添加测试Log,目的通过这个log的插入和删除测试删除操作
            LogModel testlog = new LogModel()
            {
                Site_id = 666,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = false,
                Log_Record = ""
            };
            logDAO.InsertOneLog(testlog);
            Assert.AreEqual(1, logDAO.DeleteOneLog(testlog.Id));
            Assert.AreEqual(0, logDAO.GetLogById(testlog.Id).Id);//取一下testlog，看是否取到空log   
        }
        /// <summary>
        /// 测试删除一条不存在的id
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DeleteOneLog_NotExist_Id()
        {
            Assert.AreEqual(0, logDAO.DeleteOneLog(55));

        }
        /// <summary>
        /// 用预先插入的Log的Site_id进行测试根据site_id进行删除
        /// 说明：先插入一条testlog，对比根据test_id删除的返回值是否为1，然后再根据testlog的id进行查找看是否为空；
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DeleteLogsBySite()
        {
            LogModel testlog = new LogModel()
            {
                Site_id = 123,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = false,
                Log_Record = ""
            };
            logDAO.InsertOneLog(testlog);
            Assert.AreEqual(1, logDAO.DeleteLogsBySite(testlog.Site_id));
            Assert.AreEqual(0,logDAO.GetLogById(testlog.Id).Id);//取一下testlog，看取得的是否为空log
        }
        /// <summary>
        /// 查询一条不存在的site_id
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DeleteLogsBySite_Error_site_id()
        {
            Assert.AreEqual(0, logDAO.DeleteLogsBySite(55));
        }
        /// <summary>
        /// 测试更新一条Log
        /// 说明：插入一条testlog然后将其Is_error改为true，再进行updatelog，然后根据id查找对比Is_error是否更新
        /// </summary>
        [TestMethod]
        [Priority(3)]
        public void TestLogDAO_UpdateLog()
        {
            LogModel testlog = new LogModel()
            {
                Site_id = 666,
                Status_code = "200",
                TimeCost = 200,
                Create_Time = DateTime.Now,
                Is_error = false,
                Log_Record = ""
            };
            logDAO.InsertOneLog(testlog);//将testlog插入
            testlog.Is_error = true;//改变testlog的Is_error为true
            testlog.Log_Record = "bug";//改变testlog的Log_Record为“bug”
            logDAO.UpdateLog(testlog);
            Assert.AreEqual(true, logDAO.GetLogById(testlog.Id).Is_error);//对比updatelog后testlog的Is_error是否改变
            Assert.AreEqual("bug", logDAO.GetLogById(testlog.Id).Log_Record);//对比updatelog后的log_record是否改变
            logDAO.DeleteOneLog(testlog.Id);//清除插入数据
        }
        /// <summary>
        /// 测试更新一个Log列表
        /// </summary>
        [TestMethod]
        [Priority(3)]
        public void TestLogDAO_UpdateListLog()
        {
            logDAO.InsertListLog(logModels);
            logModels[0].Log_Record = "ok";//将插入的logmodel[0]的Log_Record字段改为ok
            logModels[1].Is_error = false;//将logmodel[1]的is_error字段改为false
            logDAO.UpdateListLog(logModels);
            Assert.AreEqual("ok", logDAO.GetLogById(logModels[0].Id).Log_Record);//对比Log_Record字段是否发生改变
            Assert.AreEqual(false, logDAO.GetLogById(logModels[1].Id).Is_error);
            logDAO.DeleteLogsBySite(88);//清除插入数据

        }
        /// <summary>
        /// 测试插入一个错误Log
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
            Assert.AreNotEqual(null, logDAO.DBExcuteLogCommand("select *  from Log", o));
        }
        /// <summary>
        /// 测试执行一条错误的语句，测试DBExcuteLogCommand是否异常
        /// </summary>
        [TestMethod]
        public void TestLogDAO_DBExcuteLogCommand_Command_error()
        {
            object[] o = new object[] { };
            try
            {
                logDAO.DBExcuteLogCommand("select Sitee from Log", o);//执行一条错误语句
            }
            catch (Exception ex)
            {
                Assert.AreEqual("SQLite.Net.SQLiteException", ex.GetType().ToString());//看异常是否为SQLiteException

            }
            
        }
    }
}
