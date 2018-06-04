﻿using ServerMonitor.Models;
using ServerMonitor.Services.RequestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.ViewModels.BLL
{
    /// <summary>
    /// 站点详情界面工具类抽象接口  创建者: xb 创建时间：2018/05/10
    /// </summary>
    public interface ISiteDetailUtil
    {
        /// <summary>
        /// 请求服务器状态 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="serverProtocol"></param>
        /// <returns></returns>
        Task<LogModel> AccessDNSServer(SiteModel site,DNSRequest request);
        /// <summary>
        /// 处理请求记录 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="log"></param>
        /// <param name="request"></param>
        void CreateLogWithRequestServerResult(LogModel log, BasicRequest request);
        /// <summary>
        /// 更新指定站点状态 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="site">指定站点</param>
        /// <param name="log">请求的结果</param>
        void UpdateSiteStatus(SiteModel site, LogModel log);
        /// <summary>
        /// 截取url部分判断是否能转换成ip 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        Task<IPAddress> GetIPAddressAsync(string url);
        /// <summary>
        /// 快速排序主体 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="a">待排数组</param>
        /// <param name="low">从哪开始</param>
        /// <param name="high">到哪截至</param>
        /// <returns>排好序的数组</returns>
        void QuickSort(ref double[] a, int low, int high);
        /// <summary>
        /// 请求FTP服务器的状态 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        Task<LogModel> AccessFTPServer(SiteModel site, FTPRequest request);
        /// <summary>
        /// 请求SSH服务器的状态 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        Task<LogModel> AccessSSHServer(SiteModel site, SSHRequest request);
        /// <summary>
        /// 请求SMTP服务器的状态 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        Task<LogModel> AccessSMTPServer(SiteModel site, SMTPRequest request);
        /// <summary>
        /// 使用Socket 与服务器建立连接 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <returns></returns>
        Task<LogModel> ConnectToServerWithSocket(SiteModel site, SocketRequest request);
        /// <summary>
        /// 使用ICMP 与服务器建立连接 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <returns></returns>
        Task<LogModel> ConnectToServerWithICMP(SiteModel site, ICMPRequest request);
        /// <summary>
        /// 请求网站，并存入一条记录 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <returns></returns>
        Task<LogModel> RequestHTTPSite(SiteModel site, HTTPRequest request);
        /// <summary>
        /// 查看是否满足用户提出的成功Code 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="site"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        bool SuccessCodeMatch(SiteModel site, string statusCode);
        /// <summary>
        /// 获取服务器状态成功的状态码列表 创建者: xb 创建时间：2018/05/10
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        string[] GetSuccStatusCode(SiteModel site);
        /// <summary>
        /// 发起请求主体
        /// </summary>
        /// <returns>请求结果Log</returns>
        Task<LogModel> MakeRequest(SiteModel site);
        /// <summary>
        /// 根据给定的DNS期待返回值字符串返回期待值集合
        /// </summary>
        /// <param name="expectString"></param>
        /// <returns></returns>
        HashSet<string> GetDNSExpectResult(string expectString);
    }
}
