using Heijden.Dns.Portable;
using Heijden.DNS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitor.Services.RequestServices
{
    public class DNSRequest : BasicRequest,IRequest
    {
        // 继承的属性：CreateTime TimeCost OverTime Status Others ErrorException
        /// <summary>
        /// Dns解析记录类型,默认是A记录
        /// </summary>
        QType recordType = QType.A;
        /// <summary>
        /// 测试服务器状态使用的域名
        /// </summary>
        string domainName;
        /// <summary>
        /// 请求说明信息
        /// </summary>
        string requestInfos;
        /// <summary>
        /// 测试期待值
        /// </summary>
        HashSet<string> actualResult = null;
        /// <summary>
        /// Dns服务器IP地址
        /// </summary>
        IPAddress dnsServer;
        /// <summary>
        /// 线程安全的请求对象 --完全延迟加载
        /// </summary>
        public static DNSRequest Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        private DNSRequest() { }

        public QType RecordType { get => recordType; set => recordType = value; }
        public string DomainName { get => domainName; set => domainName = value; }
        public IPAddress DnsServer { get => dnsServer; set => dnsServer = value; }
        public HashSet<string> ActualResult { get => actualResult;}
        public string RequestInfos { get => requestInfos; }

        /// <summary>
        /// 生成一个Dns请求对象
        /// </summary>
        /// <param name="DnsServer">用于解析域名的Dns服务器</param>
        /// <param name="DomainName">待解析的域名</param>
        public DNSRequest(IPAddress DnsServer, string DomainName)
        {
            this.DnsServer = DnsServer;
            this.DomainName = DomainName;
        }

        /// <summary>
        /// Dns请求
        /// </summary>
        /// <returns></returns>
        public async Task<bool> MakeRequest()
        {
            // 对输入的参数进行有效性校验
            if (null == dnsServer|| string.IsNullOrEmpty(DomainName))
            {
                // Dns服务器请求出现未捕获到的异常
                Status = "1001";
                // 收集捕获到的异常
                ErrorException = new ArgumentNullException("Dns Server IP value or DomainName value is null!");
                // 请求耗时设置为超时上限
                TimeCost = (short)(OverTime * ErrorQuality);
                requestInfos = "Dns Server IP value or DomainName value is null!";
                return false;
            }
            // 赋值生成请求的时间
            CreateTime = DateTime.Now;
            // 创建解析使用的Dns服务器
            var resolver = new Resolver(DnsServer, 53)
            {
                // 设置请求过程的超时控制
                Timeout = TimeSpan.FromSeconds(OverTime)
            };
            Response response = null;

            // 超时控制
            CancellationTokenSource cts = new CancellationTokenSource();
            // 这里二次封装是为了引入超时控制
            try
            {
                // 记录请求耗时
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                // 二次封装任务，目的在于让请求过程变成可控的
                Task queryTask = Task.Run(async () =>
                {
                    response = await resolver.Query(DomainName, RecordType).ConfigureAwait(false);
                }, cts.Token);
                // 开启另一个任务同时进行用于记录是否超时
                var ranTask = Task.WaitAny(queryTask, Task.Delay(OverTime));
                if (0 != ranTask)
                {
                    // 取消任务，并返回超时异常
                    cts.Cancel();
                    stopwatch.Stop();
                    TimeCost = OverTime;
                    Status = "1002";
                    Exception e = new TaskCanceledException("Overtime");
                    requestInfos = e.ToString();
                    ErrorException = e;

                    // 清除请求缓存
                    resolver.ClearCache();
                    return false;
                }
                await Task.CompletedTask;
                stopwatch.Stop();

                if (response.Answers.Count != 0 && queryTask.IsCompleted) // 请求成功，获取到了解析结果
                {
                    // Dns服务器状态良好
                    Status = "1000";
                    // 请求耗时应该在2^15-1(ms)内完成
                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                    // 记录解析记录
                    actualResult = new HashSet<string>();
                    foreach (var item in response.Answers)
                    {
                        if (item.Type.ToString().Equals(recordType.ToString())) {
                            actualResult.Add(item.RECORD.ToString());
                        }
                    }
                    requestInfos = "Succeed!";
                    return true;
                }
                else // 请求失败，无解析结果
                {
                    // Dns服务器状态未知，但是该域名无法解析
                    Status = "1001";
                    // 请求耗时应该在2^15-1(ms)内完成
                    TimeCost = (short)(OverTime * ErrorQuality);
                    actualResult.Add("No Data!");
                    requestInfos = "Request Failed!";
                    return false;
                }
            }
            // 捕获到请求超时的情况
            catch (TaskCanceledException e)
            {
                // Dns服务器超时
                Status = "1002";
                // 收集捕获到的异常
                ErrorException = e;
                // 请求耗时设置为超时上限
                TimeCost = OverTime;
                requestInfos = e.Message;
                return false;
            }
            // 这个是TaskCanceledException的基类
            catch (OperationCanceledException e)
            {
                // Dns服务器超时
                Status = "1002";
                // 收集捕获到的异常
                ErrorException = e;
                // 请求耗时设置为超时上限
                TimeCost = OverTime;
                requestInfos = e.Message;
                return false;
            }
            // 用于后期做异常捕获延伸
            catch (Exception e)
            {
                // Dns服务器请求出现未捕获到的异常
                Status = "1001";
                // 收集捕获到的异常
                ErrorException = e;
                // 请求耗时设置为超时上限
                TimeCost = (short)(OverTime * ErrorQuality);
                requestInfos = e.Message;
                return false;
            }
        }

        /// <summary>
        /// 判断域名是否合法
        /// </summary>
        /// <param name="Domainname"></param>
        /// <returns></returns>
        private bool IsDomainnameCorrect(string Domainname)
        {
            if (string.IsNullOrEmpty(Domainname))
            {
                return false;
            }
            else
            {
                // 判断域名是否合法 ...
                return Uri.IsWellFormedUriString(Domainname, UriKind.Absolute);
            }

        }

        /// <summary>
        /// 检查expectResult是否命中解析结果resultSet
        /// </summary>
        /// <param name="expectResult"></param>
        /// <param name="resultSet"></param>
        /// <returns></returns>
        public bool IsMatchResult(string expectResult,HashSet<string> resultSet)
        {
            return resultSet.Contains(expectResult);
        }

        /// <summary>
        /// 用来获取枚举值得下标
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static QType GetQTypeWithIndex(int index) {
            return (QType)Enum.Parse(typeof(QType), index.ToString());
        }

        public static QType GetQTypeWithStringTypeName(string typeName) {
            return (QType)Enum.Parse(typeof(QType), typeName);
        }

        /// <summary>
        /// 用于控制线程安全的内部类
        /// </summary>
        private class Nested
        {
            static Nested()
            {

            }
            internal static readonly DNSRequest instance = new DNSRequest();
        }
    }
}
