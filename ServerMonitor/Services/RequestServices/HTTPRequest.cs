using Newtonsoft.Json;
using ServerMonitor.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitor.Services.RequestServices
{
    /**
     * Http 请求模块
     */
    public class HTTPRequest : BasicRequest
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
        /// 网页站点返回的信息
        /// </summary>
        private string webResponse = null;
        

        /// <summary>
        /// 发起一次HTTP请求，返回状态码和请求时间(ms)
        /// </summary>
        /// <param name="uri">请求的URI</param>
        public async static Task<string> HttpRequest(string uri)
        {
            HttpReturn _return = new HttpReturn();
            try
            {
                // 生成默认请求的处理帮助器
                HttpClientHandler handler = new HttpClientHandler
                {
                    // 设置程序是否跟随重定向
                    AllowAutoRedirect = false
                };
                //// 生成自定义的请求处理器
                //CustomHandler cu = new CustomHandler
                //{
                //    InnerHandler = handler
                //};
                //HttpClient client = new HttpClient(cu);
                HttpClient client = new HttpClient();
                //设置标头
                client.DefaultRequestHeaders.Referrer = new Uri(uri);
                CancellationTokenSource ctx = new CancellationTokenSource();
                ctx.CancelAfter(TimeSpan.FromSeconds(5));//5s放弃请求
                DateTime start_time = DateTime.Now;
                HttpResponseMessage message = await client.GetAsync(uri, ctx.Token);
                if (message.IsSuccessStatusCode)
                {
                    DateTime end_time = DateTime.Now;
                    TimeSpan timeSpan = end_time - start_time;
                    int chtime = (int)timeSpan.TotalMilliseconds;
                    _return.RequestTime = chtime;
                    int status_code_num = (int)Enum.Parse(typeof(System.Net.HttpStatusCode), message.StatusCode.ToString());//状态码strign to num
                    _return.StatusCode = status_code_num.ToString();
                    _return.Color = "1";
                    string backJson = JsonConvert.SerializeObject(_return);
                    return backJson;
                }
                else
                {
                    DateTime end_time = DateTime.Now;//请求失败计算时间
                    TimeSpan timeSpan = end_time - start_time;
                    int chtime = (int)timeSpan.TotalMilliseconds;
                    _return.RequestTime = chtime;
                    //请求失败状态码
                    int status_code_num = (int)Enum.Parse(typeof(System.Net.HttpStatusCode), message.StatusCode.ToString());//状态码strign to num
                    _return.StatusCode = status_code_num.ToString();
                    _return.Color = "0";
                    string backJson = JsonConvert.SerializeObject(_return);
                    return backJson;
                }
            }
            catch (TaskCanceledException e)
            {
                Debug.WriteLine("请求超时");
                DBHelper.InsertErrorLog(e);
                _return.Color = "-1";
                _return.RequestTime = 5000;
                _return.StatusCode = "1002";
                //return "请求超时";
                string backJson = JsonConvert.SerializeObject(_return);
                return backJson;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("请求失败" + ex.Message);
                DBHelper.InsertErrorLog(ex);
                _return.Color = "2";
                _return.RequestTime = -1;
                //return "请求失败";
                string backJson = JsonConvert.SerializeObject(_return);
                return backJson;
            }
        }

        public override Task<bool> MakeRequest()
        {
            throw new NotImplementedException();
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
