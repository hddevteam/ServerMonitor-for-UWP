using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.Controls;
using ServerMonitor.DAO;
using ServerMonitor.Models;
using ServerMonitor.SiteDb;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace ServerMonitor.LogDb
{
    public class LogDaoImpl : ILogDAO
    {
        /// <summary>
        /// 执行有返回对象(Log)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<LogModel> DBExcuteLogCommand(string command, object[] param)
        {
            List<LogModel> result;

            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                SQLiteCommand cmd = conn.CreateCommand(command, param);
                result = cmd.ExecuteQuery<LogModel>();
            }
            return result;
        }
        /// <summary>
        /// 删除站点关联的日志
        /// </summary>
        /// <param name="siteId">站点ID</param>
        /// <returns>删除站点关联的日志条数</returns>
        public int DeleteLogsBySite(int siteId)
        {
            int result = -1;
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
                {
                    result = conn.Execute("delete from Log where Site_id = ?", siteId);
                }
                Debug.WriteLine(string.Format("站点：{0} 成功删除 {1} 条记录！", siteId, result));
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
        /// 删除一条记录
        /// </summary>
        /// <param name="LogId"></param>
        /// <returns></returns>
        public int DeleteOneLog(int LogId)
        {
            int result = -1;

            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Execute("delete from Log where Id = ?", LogId);
            }
            return result;
        }

        /// <summary>
        /// 返回所有的日志
        /// </summary>
        /// <returns></returns>
        public  List<LogModel> GetAllLog()
        {
            List<LogModel> r;

            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                r = conn.Table<LogModel>().ToList<LogModel>();
            }
            return r;
        }
        /// <summary>
        /// 根据记录id获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public  LogModel GetLogById(int id)
        {
            LogModel l;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
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
        public List<LogModel> GetLogsBySiteId(int id)
        {
            List<LogModel> l;

            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
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
        /// 插入一条错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public int InsertErrorLog(Exception e)
        {
            int result = -1;

            //构成一个错误日志对象
            ErrorLogModel log = new ErrorLogModel()
            {
                ExceptionType = e.GetType().ToString(),
                CreateTime = DateTime.Now,
                ExceptionContent = e.Message,
                Others = string.Format("Exception Source : {0} \t Exception HResult : {1}", e.Source, e.HResult)
            };

            // 开始插入
            try
            {

                using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
                {
                    result = conn.Insert(log);
                    Debug.WriteLine("写入错误日志:");
                }
            }
            catch (Exception)
            {
                // 出现异常则在控制台输出!
                Debug.WriteLine("写入错误日志失败!日志内容为：" + log.ToString());
            }
            return result;
        }
        /// <summary>
        /// 插入多条记录
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public int InsertListLog(List<LogModel> logs)
        {
            int result = -1;

            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.InsertAll(logs);
            }
            return result;
        }
        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public int InsertOneLog(LogModel log)
        {

            int result = 0;//如果插入的Log == null 则返回 0
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Insert(log);
            }
            return result;
        }
        /// <summary>
        /// 更新信息列表
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        public int UpdateListLog(List<LogModel> logs)
        {
            int result = -1;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.UpdateAll(logs);
            }
            return result;
        }
        /// <summary>
        /// 更新记录信息
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public int UpdateLog(LogModel log)
        {
            int result;
            using (SQLiteConnection conn = new SQLiteConnection(new SQLitePlatformWinRT(), DataBaseControlImpl.DBPath))
            {
                result = conn.Update(log);
            }
            return result;
        }
    }

}
