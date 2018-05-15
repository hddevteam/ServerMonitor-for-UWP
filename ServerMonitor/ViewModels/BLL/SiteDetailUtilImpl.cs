using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
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
        public async Task<LogModel> AccessDNSServer(SiteModel site,DNSRequest request)
        {
            // 作为返回参数的请求结果记录
            LogModel log = null;           
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                // 检测并赋值DNS服务器IP
                IPAddress ip = await GetIPAddressAsync(site.Site_address);
                if (null == ip)
                {
                    try
                    {
                        // 获取请求站点的地址
                        ip = IPAddress.Parse(site.Site_address);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.WriteLine(e.ToString());
                        DBHelper.InsertErrorLog(e);
                        return null;
                    }
                }
                request.DnsServer = ip;
                #region 初始化log
                log = new LogModel
                {
                    Site_id = site.Id,
                    Create_time = DateTime.Now

                };
                #endregion
                // 赋值请求站点预计的请求结果
                // 这里待定！！！！！因为保存用于测试的域名不知道存储在哪？
                request.DomainName = site.ProtocolIdentification;
                // 开始请求
                bool result = await request.MakeRequest();

                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_record = request.RequestInfos;
                // 这里待定！！！！！因为用于保存期待结果的变量暂时不知道是如何存储的
                request.IsMatchResult(request.ActualResult.First(), new HashSet<string>() { "127.0.0.1" });
                // 更新站点信息
                UpdateSiteStatus(site, log);
            }
            return log;
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
            log.Request_time = request.TimeCost;
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
                reIP = null;
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
            site.Last_request_result = log.Is_error ? 0 : 1;
            site.Request_interval = (int)log.Request_time;
            site.Request_count++;
            // 更新数据库中的站点的信息
            DBHelper.UpdateSite(site);
            Debug.WriteLine("请求了一次服务器!");
        }
        /// <summary>
        /// 请求FTP服务器的状态
        /// </summary>
        /// <param name="site">待请求的站点</param>
        /// <param name="request">请求对象</param>
        public async Task AccessFTPServer(SiteModel site, FTPRequest request)
        {
            if (null != site.Site_address && !("".Equals(site.Site_address)))
            {
                // 检测并赋值DNS服务器IP
                IPAddress ip = await GetIPAddressAsync(site.Site_address);
                if (null == ip)
                {
                    try
                    {
                        // 获取请求站点的地址
                        ip = IPAddress.Parse(site.Site_address);
                    }
                    catch (ArgumentException e)
                    {
                        Debug.WriteLine(e.ToString());
                        DBHelper.InsertErrorLog(e);
                        
                    }
                }
                request.FtpServer = IPAddress.Parse(site.Site_address);
                // 这里待定！！！！！因为用于保存用户身份识别信息的变量未知！！
                request.Identification = new IdentificationInfo() { Username = site.ProtocolIdentification, Password = site.ProtocolIdentification };
                #region 初始化log
                LogModel log = new LogModel
                {
                    Site_id = site.Id,
                    Create_time = DateTime.Now

                };
                #endregion                
                // 开始请求
                bool result = await request.MakeRequest();
                // 处理请求记录
                CreateLogWithRequestServerResult(log, request);
                // 补充额外添加的判断
                log.Log_record = request.ProtocalInfo;               
                // 更新站点信息
                UpdateSiteStatus(site, log);
            }
        }

        public Task AccessSSHServer(SiteModel site, SSHRequest requuest)
        {
            throw new NotImplementedException();
        }

        public Task AccessSMTPServer(SiteModel site, SSHRequest requuest)
        {
            throw new NotImplementedException();
        }
    }
}
