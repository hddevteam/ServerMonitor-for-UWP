using ServerMonitor.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ServerMonitor.Controls
{
    public class DBHelper
    {
        private static string DbFilename;
        private static string DBPath;
        public static string DbFilename1 { get => DbFilename; set => DbFilename = value; }


        public static void SetDBFilename(string Filename)
        {
            if (string.IsNullOrEmpty(Filename) && string.IsNullOrWhiteSpace(Filename))
            {
                throw new ArgumentNullException("操作数据库名称不合法!");
            }
            else
            {
                DbFilename1 = Filename;
                DBPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, DbFilename1);
            }
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        /// <param name="DBFilename">数据库名称</param>
        public static void InitDB(string DBFilename)
        {
            SetDBFilename(DBFilename);
            // ApplicationData.Current.LocalFolder.Path balabala的指的是这个位置 ->C:\Users\xiao22805378\AppData\Local\Packages\92211ab1-5481-4a1a-9111-a3dd87b81b72_8zmgqd0netmce\LocalState\
            if (!File.Exists(DBPath))
            {
                // ApplicationData.Current.LocalFolder.Path balabala的指的是这个位置 ->C:\Users\xiao22805378\AppData\Local\Packages\92211ab1-5481-4a1a-9111-a3dd87b81b72_8zmgqd0netmce\LocalState\
                using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
                {
                    conn.CreateTable<SiteModel>();
                    conn.CreateTable<LogModel>();
                    conn.CreateTable<ErrorLogModel>();
                    conn.CreateTable<ContactModel>();
                    conn.CreateTable<SiteContactModel>();
                    List<SiteModel> l_site = new List<SiteModel>
                    {
                        // 插入默认的五条数据
                        new SiteModel()
                        {
                            Site_name = "Google",
                            Site_address = "https://www.google.com",
                            Is_server = false,
                            Protocol_type = "HTTPS",
                            Server_port = 1,
                            Monitor_interval = 5,
                            Is_Monitor = false,
                            Status_code = "200",
                            Request_TimeCost = 25383,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "200",
                            Is_success = 0
                        },
                        new SiteModel()
                        {
                            Site_name = "Yahoo",
                            Site_address = "http://www.yahoo.com",
                            Is_server = false,
                            Protocol_type = "HTTP",
                            Server_port = 1,
                            Monitor_interval = 5,
                            Is_Monitor = true,
                            Status_code = "200",
                            Request_TimeCost = 11851,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "200",
                            Is_success = 1
                        },
                        new SiteModel()
                        {
                            Site_name = "Bing",
                            Site_address = "http://www.bing.com",
                            Is_server = false,
                            Protocol_type = "HTTP",
                            Server_port = 1,
                            Monitor_interval = 5,
                            Is_Monitor = false,
                            Status_code = "200",
                            Request_TimeCost = 287,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "200",
                            Is_success = -1
                        },
                        new SiteModel()
                        {
                            Site_name = "Google",
                            Site_address = "8.8.8.8",
                            Is_server = true,
                            Protocol_type = "DNS",
                            Server_port = 53,
                            Monitor_interval = 5,
                            Is_Monitor = true,
                            Status_code = "1000/0",
                            Request_TimeCost = 11,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = true,
                            Request_succeed_code = "1000",
                            Is_success = 2
                        },
                        new SiteModel()
                        {
                            Site_name = "Google",
                            Site_address = "https://www.google.com",
                            Is_server = true,
                            Protocol_type = "ICMP",
                            Server_port = 53,
                            Monitor_interval = 5,
                            Is_Monitor = false,
                            Status_code = "1000/0",
                            Request_TimeCost = 25383,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "1000",
                            Is_success = 2
                        }
                    };
                    //for (int i = 0; i < 30; i++)
                    //{
                    //    l_site.Add(new Site() { Site_name = "Baidu" + i, Is_server = false, Request_succeed_code = "200" });
                    //}
                    InsertListSite(l_site);
                    List<LogModel> l_log = new List<LogModel>
                    {
                        new LogModel()
                        {
                            Site_id = 1,
                            Status_code = "200",
                            TimeCost = 30,
                            Create_Time = DateTime.Now.AddDays(-2),
                            Is_error = false,
                            Log_Record = null
                        },
                        new LogModel()
                        {
                            Site_id = 2,
                            Status_code = "200",
                            TimeCost = 20,
                            Create_Time = DateTime.Now,
                            Is_error = false,
                            Log_Record = null
                        },
                        new LogModel()
                        {
                            Site_id = 3,
                            Status_code = "1000",
                            TimeCost = 40,
                            Create_Time = DateTime.Now.AddDays(-5),
                            Is_error = false,
                            Log_Record = null
                        },
                        new LogModel()
                        {
                            Site_id = 4,
                            Status_code = "1000",
                            TimeCost = 30,
                            Create_Time = DateTime.Now.AddDays(-3),
                            Is_error = false,
                            Log_Record = null
                        },
                        new LogModel()
                        {
                            Site_id = 5,
                            Status_code = "1000",
                            TimeCost = 10,
                            Create_Time = DateTime.Now.AddDays(-6),
                            Is_error = false,
                            Log_Record = null
                        }
                    };
                    InsertListLog(l_log);
                }
            }
            else
            {
                /**
                 * 放处理数据库已经存在的处理代码
                 */
            }
        }


        /// <summary>
        /// 返回所有的站点
        /// </summary>
        /// <returns></returns>
        public static List<SiteModel> GetAllSite()
        {
            List<SiteModel> r;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                r = conn.Table<SiteModel>().ToList<SiteModel>();
            }
            return r;

        }

        /// <summary>
        /// 返回所有的日志
        /// </summary>
        /// <returns></returns>
        public static List<LogModel> GetAllLog()
        {
            List<LogModel> r;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                r = conn.Table<LogModel>().ToList<LogModel>();
            }
            return r;
        }

        /// <summary>
        /// 根据站点id获取站点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SiteModel GetSiteById(int id)
        {
            SiteModel s;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                try
                {
                    s = conn.Table<SiteModel>().Where(v => v.Id == id).ToList<SiteModel>()[0];
                }
                catch
                {
                    s = new SiteModel();
                }
            }
            return s;
        }

        /// <summary>
        /// 根据记录id获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static LogModel GetLogById(int id)
        {
            LogModel l;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                try
                {
                    l = conn.Table<LogModel>().Where(v => v.Id == id).ToList<LogModel>()[0];
                }
                catch
                {
                    l = new LogModel();
                }
            }
            return l;
        }

        /// <summary>
        /// 根据站点id获取日志
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<LogModel> GetLogsBySiteId(int id)
        {
            List<LogModel> l;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                try
                {
                    l = conn.Table<LogModel>().Where(v => v.Site_id == id).ToList<LogModel>();
                }
                catch
                {
                    l = new List<LogModel>();
                }
            }
            return l;
        }

        /// <summary>
        /// 插入一个站点
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static int InsertOneSite(SiteModel site)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Insert(site);
            }
            return result;
        }

        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static int InsertOneLog(LogModel log)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Insert(log);
            }
            return result;
        }

        /// <summary>
        /// 插入多条站点
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public static int InsertListSite(List<SiteModel> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.InsertAll(sites);
            }
            return result;
        }

        /// <summary>
        /// 插入多条记录
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public static int InsertListLog(List<LogModel> logs)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.InsertAll(logs);
            }
            return result;
        }

        /// <summary>
        /// 删除一个站点
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public static int DeleteOneSite(int siteId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Execute("delete from Site where Id = ?", siteId);
            }
            return result;
        }

        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="LogId"></param>
        /// <returns></returns>
        public static int DeleteOneLog(int LogId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Execute("delete from Log where Id = ?", LogId);
            }
            return result;
        }

        /// <summary>
        /// 删除站点关联的日志
        /// </summary>
        /// <param name="siteId">站点ID</param>
        /// <returns>删除站点关联的日志条数</returns>
        public static int DeleteLogsBySite(int siteId) {
            int result = -1;            
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
                {
                    result = conn.Execute("delete from Log where Site_id = ?", siteId);
                }
                Debug.WriteLine(string.Format("站点：{0} 成功删除 {1} 条记录！", siteId,result));
            }
            // 若捕获到数据库相关的异常，如未找到此条记录
            catch (SQLite.Net.SQLiteException e)
            {
                result = -1;
                InsertErrorLog(e);
            }

            return result;
        }

        /// <summary>
        /// 更新站点信息
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public static int UpdateSite(SiteModel site)
        {
            // result = -1 表示异常返回值，执行操作失败
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                try
                {
                    result = conn.Update(site);
                    Debug.WriteLine(string.Format("更新站点{0} 成功！站点当前已经请求{1}次",site.Id,site.Request_count));
                }
                // 若捕获到数据库相关的异常，如未找到此条记录
                catch (SQLite.Net.SQLiteException e) {                    
                    result = -1;
                    InsertErrorLog(e);
                }
                
            }
            return result;
        }

        /// <summary>
        /// 更新记录信息
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static int UpdateLog(LogModel log)
        {
            int result;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Update(log);
            }
            return result;
        }

        /// <summary>
        /// 更新站点列表
        /// </summary>
        /// <param name="sites"></param>
        /// <returns></returns>
        public static int UpdateListSite(List<SiteModel> sites)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.UpdateAll(sites);
            }
            return result;
        }

        /// <summary>
        /// 更新信息列表
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public static int UpdateListLog(List<LogModel> logs)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.UpdateAll(logs);
            }
            return result;
        }

        /// <summary>
        /// 插入一条错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public static int InsertErrorLog(Exception e) {
            int result = -1;

            //构成一个错误日志对象
            ErrorLogModel log = new ErrorLogModel() {
                ExceptionType = e.GetType().ToString(),
                CreateTime = DateTime.Now,
                ExceptionContent = e.Message,
                Others = string.Format("Exception Source : {0} \t Exception HResult : {1}", e.Source, e.HResult)
            };

            // 开始插入
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
                {
                    result = conn.Insert(log);
                    Debug.WriteLine("写入错误日志:");
                }
            }
            catch (Exception) {
                // 出现异常则在控制台输出!
                Debug.WriteLine("写入错误日志失败!日志内容为：" + log.ToString());
            }
            return result;
        }

        /// <summary>
        /// 执行无返回对象的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static int DBExcuteNonCommand(string command, object[] param)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                SQLiteCommand cmd = conn.CreateCommand(command, param);
                result = cmd.ExecuteNonQuery();
            }
            return result;
        }

        /// <summary>
        /// 执行有返回对象(Site)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<SiteModel> DBExcuteSiteCommand(string command, object[] param)
        {
            List<SiteModel> result;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                SQLiteCommand cmd = conn.CreateCommand(command, param);
                result = cmd.ExecuteQuery<SiteModel>();
            }
            return result;
        }

        /// <summary>
        /// 执行有返回对象(Log)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static List<LogModel> DBExcuteLogCommand(string command, object[] param)
        {
            List<LogModel> result;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                SQLiteCommand cmd = conn.CreateCommand(command, param);
                result = cmd.ExecuteQuery<LogModel>();
            }
            return result;
        }

        #region Contact表操作
        /// <summary>
        /// 返回所有的联系人
        /// </summary>
        /// <returns></returns>
        public static List<ContactModel> GetAllContact()
        {
            List<ContactModel> r;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                r = conn.Table<ContactModel>().ToList<ContactModel>();
            }
            return r;
        }        

        /// <summary>
        /// 插入一条联系人
        /// </summary>
        /// <param name="Contact"></param>
        /// <returns></returns>
        public static int InsertOneContact(ContactModel contact)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Insert(contact);
            }
            return result;
        }

        /// <summary>
        /// 删除一个联系人
        /// </summary>
        /// <param name="contactId"></param>
        /// <returns></returns>
        public static int DeleteOneContact(int contactId)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                result = conn.Execute("delete from Contact where Id = ?", contactId);
            }
            return result;
        }

        /// <summary>
        /// 更新联系人信息
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public static int UpdateContact(ContactModel contact)
        {
            // result = -1 表示异常返回值，执行操作失败
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
            {
                try
                {
                    result = conn.Update(contact);
                }
                // 若捕获到数据库相关的异常，如未找到此条记录
                catch (SQLite.Net.SQLiteException e)
                {
                    result = -1;
                    InsertErrorLog(e);
                }

            }
            return result;
        }
        #endregion


        /// <summary>
        /// 自定义的toString方法
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "数据库名称:" + DbFilename + "当前时间:" + DateTime.Now.ToString();
        }

        
    }
}
