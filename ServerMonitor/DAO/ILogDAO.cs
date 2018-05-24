using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.LogDb
{
    public interface ILogDAO
    {
        /// <summary>
        /// 返回所有的日志
        /// </summary>
        /// <returns></returns>
        List<LogModel> GetAllLog();
        /// <summary>
        /// 根据记录id获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        LogModel GetLogById(int id);
        /// <summary>
        /// 根据站点id获取日志
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        List<LogModel> GetLogsBySiteId(int id);
        /// <summary>
        /// 插入一条记录
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        int InsertOneLog(LogModel log);
        /// <summary>
        /// 插入多条记录
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        int InsertListLog(List<LogModel> logs);
        /// <summary>
        /// 删除一条记录
        /// </summary>
        /// <param name="LogId"></param>
        /// <returns></returns>
        int DeleteOneLog(int LogId);
        /// <summary>
        /// 删除站点关联的日志
        /// </summary>
        /// <param name="siteId">站点ID</param>
        /// <returns>删除站点关联的日志条数</returns>
        int DeleteLogsBySite(int siteId);
        /// <summary>
        /// 更新记录信息
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        int UpdateLog(LogModel log);
        /// <summary>
        /// 更新信息列表
        /// </summary>
        /// <param name="logs"></param>
        /// <returns></returns>
        int UpdateListLog(List<LogModel> logs);
        /// <summary>
        /// 插入一条错误日志
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        int InsertErrorLog(Exception e);
        /// <summary>
        /// 执行有返回对象(Log)的命令
        /// </summary>
        /// <param name="command"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        List<LogModel> DBExcuteLogCommand(string command, object[] param);

    }
}
