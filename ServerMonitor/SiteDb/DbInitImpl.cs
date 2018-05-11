using ServerMonitor.Controls;
using ServerMonitor.Models;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ServerMonitor.SiteDb
{
    public class DbInitImpl : DBInit
    {
        private SiteDao siteDao;
        private static string DbFilename;
        private static string DBPath;
        public static string DbFilename1 { get => DbFilename; set => DbFilename = value; }
        public static string DBPath1 { get => DBPath; }
        public void InitDB(string DBFilename)
        {
            SetDBFilename(DBFilename);
            siteDao = new SiteDaoImpl();
            // ApplicationData.Current.LocalFolder.Path balabala的指的是这个位置 ->C:\Users\xiao22805378\AppData\Local\Packages\92211ab1-5481-4a1a-9111-a3dd87b81b72_8zmgqd0netmce\LocalState\
            if (!File.Exists(DBPath))
            {
                // ApplicationData.Current.LocalFolder.Path balabala的指的是这个位置 ->C:\Users\xiao22805378\AppData\Local\Packages\92211ab1-5481-4a1a-9111-a3dd87b81b72_8zmgqd0netmce\LocalState\
                using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DBPath))
                {
                    conn.CreateTable<Site>();
                    conn.CreateTable<Log>();
                    conn.CreateTable<ErrorLog>();
                    conn.CreateTable<Contact>();
                    List<Site> l_site = new List<Site>
                    {
                        // 插入默认的五条数据
                        new Site()
                        {
                            Site_name = "Google",
                            Site_address = "https://www.google.com",
                            Is_server = false,
                            Protocol_type = "HTTPS",
                            Server_port = 1,
                            Monitor_interval = 5,
                            Is_Monitor = false,
                            Status_code = "200",
                            Request_interval = 25383,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "200",
                            Last_request_result = 0
                        },
                        new Site()
                        {
                            Site_name = "Yahoo",
                            Site_address = "http://www.yahoo.com",
                            Is_server = false,
                            Protocol_type = "HTTP",
                            Server_port = 1,
                            Monitor_interval = 5,
                            Is_Monitor = true,
                            Status_code = "200",
                            Request_interval = 11851,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "200",
                            Last_request_result = 1
                        },
                        new Site()
                        {
                            Site_name = "Bing",
                            Site_address = "http://www.bing.com",
                            Is_server = false,
                            Protocol_type = "HTTP",
                            Server_port = 1,
                            Monitor_interval = 5,
                            Is_Monitor = false,
                            Status_code = "200",
                            Request_interval = 287,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "200",
                            Last_request_result = -1
                        },
                        new Site()
                        {
                            Site_name = "Google",
                            Site_address = "8.8.8.8",
                            Is_server = true,
                            Protocol_type = "DNS",
                            Server_port = 53,
                            Monitor_interval = 5,
                            Is_Monitor = true,
                            Status_code = "1000/0",
                            Request_interval = 11,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = true,
                            Request_succeed_code = "1000",
                            Last_request_result = 2
                        },
                        new Site()
                        {
                            Site_name = "Google",
                            Site_address = "https://www.google.com",
                            Is_server = true,
                            Protocol_type = "ICMP",
                            Server_port = 53,
                            Monitor_interval = 5,
                            Is_Monitor = false,
                            Status_code = "1000/0",
                            Request_interval = 25383,
                            Create_time = DateTime.Now,
                            Update_time = DateTime.Now,
                            Is_pre_check = false,
                            Request_succeed_code = "1000",
                            Last_request_result = 2
                        }
                    };
                    //for (int i = 0; i < 30; i++)
                    //{
                    //    l_site.Add(new Site() { Site_name = "Baidu" + i, Is_server = false, Request_succeed_code = "200" });
                    //}
                    siteDao.InsertListSite(l_site);
                    List<Log> l_log = new List<Log>
                    {
                        new Log()
                        {
                            Site_id = 1,
                            Status_code = "200",
                            Request_time = 30,
                            Create_time = DateTime.Now.AddDays(-2),
                            Is_error = false,
                            Log_record = null
                        },
                        new Log()
                        {
                            Site_id = 2,
                            Status_code = "200",
                            Request_time = 20,
                            Create_time = DateTime.Now,
                            Is_error = false,
                            Log_record = null
                        },
                        new Log()
                        {
                            Site_id = 3,
                            Status_code = "1000",
                            Request_time = 40,
                            Create_time = DateTime.Now.AddDays(-5),
                            Is_error = false,
                            Log_record = null
                        },
                        new Log()
                        {
                            Site_id = 4,
                            Status_code = "1000",
                            Request_time = 30,
                            Create_time = DateTime.Now.AddDays(-3),
                            Is_error = false,
                            Log_record = null
                        },
                        new Log()
                        {
                            Site_id = 5,
                            Status_code = "1000",
                            Request_time = 10,
                            Create_time = DateTime.Now.AddDays(-6),
                            Is_error = false,
                            Log_record = null
                        }
                    };
                    DBHelper.InsertListLog(l_log);
                }
            }
            else
            {
                /**
                 * 放处理数据库已经存在的处理代码
                 */
            }
        }

        public void SetDBFilename(string Filename)
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
    }
}
