using ServerMonitor.Controls;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitor.Services.RequestServices
{
    /**
     * Http 请求模块
     * 使用了单例模式 -- 完全延迟加载
     */
    public class HTTPRequest : BasicRequest, IRequest
    {
        // 继承的属性：CreateTime TimeCost OverTime Status Others ErrorException  
        /// <summary>
        /// http传输端口
        /// </summary>
        private const short HTTPPORT = 80;
        /// <summary>
        /// https传输端口
        /// </summary>
        private const short HTTPSPORT = 443;
        /// <summary>
        /// 请求的路由地址
        /// </summary>
        private string uri = "";
        /// <summary>
        /// 请求的说明信息
        /// </summary>
        private string requestInfo = null;
        /// <summary>
        /// 线程安全的请求对象 --完全延迟加载
        /// </summary>
        public static HTTPRequest Instance
        {
            get
            {
                return Nested.instance;
            }
        }
        /// <summary>
        /// 请求的类型 http | https ,默认是 http
        /// </summary>
        private TransportProtocol httpOrhttps = TransportProtocol.http;

        public string Uri { set => uri = value; }
        public TransportProtocol ProtocolType { set => httpOrhttps = value; }
        public string RequestInfo { get => requestInfo; }

        private HTTPRequest() { }

        /// <summary>
        /// 发起一次HTTP请求，返回状态码和请求时间(ms)
        /// </summary>
        /// <param name="uri">请求的URI</param>
        private async Task<bool> HttpRequest(string uri)
        {
            CreateTime = DateTime.Now;
            try
            {
                // 生成默认请求的处理帮助器
                HttpClientHandler handler = new HttpClientHandler
                {
                    // 设置程序是否跟随重定向
                    AllowAutoRedirect = false
                };
                // 自动释放链接资源
                using (HttpClient client = new HttpClient(handler))
                {
                    //设置标头
                    client.DefaultRequestHeaders.Referrer = new Uri(uri);
                    // 设置请求预期超时时间 
                    client.Timeout = TimeSpan.FromSeconds(OverTime);
                    // 加入请求任务超时控制
                    CancellationTokenSource ctx = new CancellationTokenSource();
                    ctx.CancelAfter(TimeSpan.FromSeconds(OverTime));//5s放弃请求
                    DateTime start_time = DateTime.Now;
                    HttpResponseMessage message = await client.GetAsync(uri, ctx.Token);

                    //请求失败计算时间
                    DateTime end_time = DateTime.Now;
                    TimeSpan timeSpan = end_time - start_time;
                    TimeCost = (short)timeSpan.TotalMilliseconds;
                    //请求失败状态码
                    Status = ((int)Enum.Parse(typeof(System.Net.HttpStatusCode), message.StatusCode.ToString())).ToString();//状态码strign to num                    
                }
                return true;
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("请求超时");
                DBHelper.InsertErrorLog(e);
                TimeCost = OverTime;
                Status = "1002";
                ErrorException = e;
                requestInfo = "Request OverTime !";
                return false;
            }
            catch (OperationCanceledException e)
            {
                Debug.WriteLine("请求失败" + e.Message);
                DBHelper.InsertErrorLog(e);
                TimeCost = -1;
                ErrorException = e;
                requestInfo = e.Message;
                return false;
            }
            catch (Exception e)
            {
                Debug.WriteLine("请求失败" + e.Message);
                DBHelper.InsertErrorLog(e);
                TimeCost = (short)(OverTime * 2);
                ErrorException = e;
                requestInfo = e.Message;
                return false;
            }
        }

        /// <summary>
        /// http请求方法
        /// </summary>
        /// <returns></returns>
        public async Task<bool> MakeRequest()
        {
            bool result = false;
            switch (httpOrhttps)
            {
                case TransportProtocol.http:
                    result = await HttpRequest(uri);
                    return result;
                default:
                    return result;
            }
        }

        /// <summary>
        /// 用于控制线程安全的内部类
        /// </summary>
        private class Nested
        {
            static Nested()
            {

            }
            internal static readonly HTTPRequest instance = new HTTPRequest();
        }
    }

    /// <summary>
    /// 用户登入识别类型
    /// </summary>
    public enum TransportProtocol
    {
        /// <summary>
        /// 超文本传输协议 hypertext transport protocol
        /// </summary>
        http,
        /// <summary>
        /// 超文本安全传输协议 hypertext transport protocol secure
        /// </summary>
        https
    };


}
