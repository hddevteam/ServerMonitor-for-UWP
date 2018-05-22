using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Controls;
using ServerMonitor.Models;
using ServerMonitor.Services.RequestServices;

namespace ServerMonitor.ViewModels.BLL
{
    public class SiteDetailUtilImpl : ISiteDetailUtil
    {
        /// <summary>
        /// 请求服务器状态
        /// </summary>
        /// <param name="serverProtocol"></param>
        /// <returns></returns>
        public async Task<LogModel> AccessDNSServer(SiteModel site, DNSRequest request)
        {
            // 作为返回参数的请求结果记录
            LogModel log = null;
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                // 检测并赋值DNS服务器IP
                IPAddress ip = await GetIPAddressAsync(site.Site_address);
                // ip 不合法
                if (null == ip)
                {
                    return null;
                }
                request.DnsServer = ip;
                #region 初始化log
                log = new LogModel
                {
                    Site_id = site.Id,
                    Create_Time = DateTime.Now

                };
                #endregion
                // 获取存储的请求预处理信息
                JObject js = (JObject)JsonConvert.DeserializeObject(site.ProtocolIdentification);
                try
                {
                    // 赋值请求类型
                    request.RecordType = DNSRequest.GetQTypeWithIndex(int.Parse(js["recordType"].ToString()));
                }
                catch (Exception)
                {
                    request.RecordType = 0;//出错 默认选第一个
                }
                // 赋值用于测试的域名
                request.DomainName = js["lookup"].ToString();
                // 开始请求
                bool result = await request.MakeRequest();

                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_Record = request.RequestInfos;
                log.Is_error = !request.IsMatchResult(request.ActualResult.First(),
                    new HashSet<string>() {
                        // 赋值保存的期待返回值
                        js["expectedResults"].ToString()
                    });
                if (log.Is_error)
                {
                    log.Status_code = "1001";
                }
                // 更新站点信息
                UpdateSiteStatus(site, log);
            }
            return log;
        }
        /// <summary>
        /// 请求FTP服务器的状态
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        public async Task<LogModel> AccessFTPServer(SiteModel site, FTPRequest request)
        {
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                // 检测并赋值DNS服务器IP
                IPAddress ip = await GetIPAddressAsync(site.Site_address);
                // IP 不合法
                if (null == ip)
                {
                    return null;
                }
                request.FtpServer = IPAddress.Parse(site.Site_address);
                // 获取保存的用户验证的信息
                JObject js = (JObject)JsonConvert.DeserializeObject(site.ProtocolIdentification);
                request.Identification = new IdentificationInfo() { Username = js["useaname"].ToString(), Password = js["password"].ToString() };
                #region 初始化log
                LogModel log = new LogModel
                {
                    Site_id = site.Id,
                    Create_Time = DateTime.Now

                };
                #endregion                
                // 开始请求
                bool result = await request.MakeRequest();
                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_Record = request.ProtocalInfo;
                // 更新站点信息
                UpdateSiteStatus(site, log);
                return log;
            }
            return null;
        }
        /// <summary>
        /// 请求SSH服务器的状态
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        public async Task<LogModel> AccessSSHServer(SiteModel site, SSHRequest request)
        {
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                request.IPAddress = site.Site_address;
                // 获取保存的用户验证的信息
                JObject js = (JObject)JsonConvert.DeserializeObject(site.ProtocolIdentification);
                request.Identification = new SshIdentificationInfo() { Username = js["useaname"].ToString(), Password = js["password"].ToString() };
                #region 初始化log
                LogModel log = new LogModel
                {
                    Site_id = site.Id,
                    Create_Time = DateTime.Now
                };
                #endregion                
                // 开始请求
                bool result = await request.MakeRequest();
                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_Record = request.ProtocolInfo;
                // 更新站点信息
                UpdateSiteStatus(site, log);
                return log;
            }
            return null;
        }
        /// <summary>
        /// 请求SMTP服务器的状态
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        public async Task<LogModel> AccessSMTPServer(SiteModel site, SMTPRequest request)
        {
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                request.DomainName = site.Site_address;
                #region 初始化log
                LogModel log = new LogModel
                {
                    Site_id = site.Id,
                    Create_Time = DateTime.Now

                };
                #endregion                
                // 开始请求
                bool result = await request.MakeRequest();
                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_Record = request.ActualResult;
                // 更新站点信息
                UpdateSiteStatus(site, log);
                return log;
            }
            return null;
        }
        /// <summary>
        /// 使用Socket 与服务器建立连接
        /// </summary>
        /// <returns></returns>
        public async Task<LogModel> ConnectToServerWithSocket(SiteModel site, SocketRequest request)
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(site.Site_address), site.Server_port);
            if (null != endPoint.Address && !("".Equals(endPoint.Address)))
            {
                request.TargetEndPoint = endPoint;
                #region 初始化log
                LogModel log = new LogModel
                {
                    Site_id = site.Id,
                    Create_Time = DateTime.Now
                };
                #endregion                
                // 开始请求
                bool result = await request.MakeRequest();
                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_Record = request.ProtocolInfo;
                // 更新站点信息
                UpdateSiteStatus(site, log);
                return log;
            }
            return null;
        }
        /// <summary>
        /// 处理请求记录
        /// </summary>
        /// <param name="log"></param>
        /// <param name="request"></param>
        public void CreateLogWithRequestServerResult(LogModel log, BasicRequest request)
        {
            // 请求成功
            log.Status_code = request.Status;
            log.TimeCost = request.TimeCost;
            // 请求失败
            switch (log.Status_code)
            {
                case "1000":
                    log.Is_error = false;
                    break;
                case "1001":
                    log.Is_error = true;
                    break;
                case "1002":
                    log.Is_error = true;
                    break;
            }
        }
        /// <summary>
        /// 更新指定站点状态
        /// </summary>
        /// <param name="site">指定站点</param>
        /// <param name="log">请求的结果</param>
        public async Task<IPAddress> GetIPAddressAsync(string url)
        {
            if (!IPAddress.TryParse(url, out IPAddress reIP))
            {
                //如果输入的不是ip地址               
                //通过域名解析ip地址
                url = url.Substring(url.IndexOf('w'));//网址截取从以第一w
                IPAddress[] hostEntry = await Dns.GetHostAddressesAsync(url);
                for (int m = 0; m < hostEntry.Length; m++)
                {
                    if (hostEntry[m].AddressFamily == AddressFamily.InterNetwork)
                    {
                        reIP = hostEntry[m];
                        break;
                    }
                }
            }
            else
            {
                reIP = IPAddress.Parse(url);
            }
            return reIP;
        }
        /// <summary>
        /// 快速排序主体
        /// </summary>
        /// <param name="a">待排数组</param>
        /// <param name="low">从哪开始</param>
        /// <param name="high">到哪截至</param>
        /// <returns>排好序的数组</returns>
        public void QuickSort(ref double[] a, int low, int high)
        {
            // 加入这个low<high ，一个是确保数组的长度大于1，二是确定递归结束的条件，防止进入死循环，栈溢出
            if (low < high)
            {
                // 每次获取中枢值的位置
                int pivotloc = Partition(ref a, low, high);
                // 利用中枢值将每遍排好序的数组分割成两份，接着从low到pivotkey-1 以及 pivotkey+1 到 high两个区域进行排序
                // 这里加入比较两端的长度，旨在降低栈的最大深度（降低至logn）
                if ((pivotloc - low) <= (high - pivotloc))
                {
                    QuickSort(ref a, low, pivotloc - 1);
                    QuickSort(ref a, pivotloc + 1, high);
                }
                else
                {
                    QuickSort(ref a, pivotloc + 1, high);
                    QuickSort(ref a, low, pivotloc - 1);
                }
            }
        }
        /// <summary>
        /// 一遍快速排序
        /// </summary>
        /// <param name="a">待排数组</param>
        /// <param name="low">排序起始值</param>
        /// <param name="high">排序最高值</param>
        /// <returns></returns>
        public int Partition(ref double[] a, int low, int high)
        {
            double pivotkey = a[low];
            while (low < high)
            {
                while (low < high && a[high] >= pivotkey)
                    high--;
                a[low] += a[high];
                a[high] = a[low] - a[high];
                a[low] -= a[high];
                while (low < high && a[low] <= pivotkey)
                    low++;
                a[low] += a[high];
                a[high] = a[low] - a[high];
                a[low] -= a[high];
            }
            a[low] = pivotkey;
            return low;
        }
        /// <summary>
        /// 截取url部分判断是否能转换成ip
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public void UpdateSiteStatus(SiteModel site, LogModel log)
        {
            // 更新站点变量的信息
            site.Status_code = log.Status_code;
            site.Update_time = DateTime.Now;
            if (log.Is_error)
            {
                site.Is_success = "1002".Equals(log.Status_code) ? -1 : 0;
            }
            else
            {
                site.Is_success = 1;
            }
            site.Request_TimeCost = (int)log.TimeCost;
            site.Request_count++;
            site.Last_response = string.IsNullOrEmpty(log.Log_Record) ? "" : log.Log_Record;
            // 更新数据库中的站点的信息
            DBHelper.UpdateSite(site);
            Debug.WriteLine("请求了一次服务器!");
        }
        /// <summary>
        /// 查看是否满足用户提出的成功Code
        /// </summary>
        /// <param name="site"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public bool SuccessCodeMatch(SiteModel site, string statusCode)
        {
            string[] successCodes = getSuccStatusCode(site);
            foreach (var i in successCodes)
            {
                if (i.Equals(statusCode))
                {
                    return true;

                }
            }
            return false;
        }
        /// <summary>
        /// 获取服务器状态成功的状态码列表
        /// </summary>
        /// <param name="site"></param>
        /// <returns></returns>
        public string[] getSuccStatusCode(SiteModel site)
        {
            if (site.Request_succeed_code.Contains(','))
            {
                return site.Request_succeed_code.Split(',');
            }
            else
            {
                return new string[] { site.Request_succeed_code };
            }
        }
        /// <summary>
        /// 请求网站，并存入一条记录
        /// </summary>
        /// <returns></returns>
        public async Task<LogModel> RequestHTTPSite(SiteModel site, HTTPRequest request)
        {
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                request.Uri = site.Site_address;
                #region 初始化log
                LogModel log = new LogModel
                {
                    Site_id = site.Id,
                    Create_Time = DateTime.Now

                };
                #endregion                
                // 开始请求
                bool result = await request.MakeRequest();
                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_Record = request.RequestInfo;
                log.Is_error = !SuccessCodeMatch(site, log.Status_code);
                // 更新站点信息
                UpdateSiteStatus(site, log);
                return log;
            }
            return null;
        }
    }
}
