using Newtonsoft.Json;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace ServerMonitor.Controls
{
    class Request
    {
        public static Dictionary<string, string> backData = new Dictionary<string, string>();
        //此类用于发起一次icmp Request
        /// <summary>
        /// 发起 ICMP request 返回请求结果
        /// </summary>
        /// <param name="iPAddress">需要ping的ipv4 ip地址</param>
        public static Dictionary<string, string> IcmpRequest(IPAddress iPAddress)
        {
            backData.Clear();
            //exception.Text = "";
            //string hostclient = webname.Text;
            if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                //传入是正确的Ipv4格式
                EndPoint hostEndpoint = (EndPoint)new IPEndPoint(iPAddress, 1025);
                //循环5次发送icmp包的操作
                for (int i = 0; i < 5; i++)
                {
                    int Datasize = 4;
                    int Packetsize = 8 + Datasize;
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                    EndPoint clientep = (EndPoint)new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30);
                    IcmpPacket packet = new IcmpPacket(8, 0, 0, 45, 0, Datasize);
                    Byte[] myBuffer = new Byte[Packetsize];
                    int index = packet.CountByte(myBuffer);
                    if (index != Packetsize)
                    {
                        //exception.Text = "报文出现问题";
                        //backData.Add("报文出现问题", "-1");
                        IcmpReturn information = new IcmpReturn();
                        information.Color = "0";//错误
                        string backJson = JsonConvert.SerializeObject(information);
                        backData.Add("1", backJson);
                        return backData;
                    }
                    int Cksum_buffer_length = (int)Math.Ceiling(((Double)index) / 2);
                    UInt16[] Cksum_buffer = new UInt16[Cksum_buffer_length];
                    int Icmp_header_buffer_index = 0;
                    for (int j = 0; j < Cksum_buffer_length; j++)
                    {
                        //把两个byte转化为一个uint16
                        Cksum_buffer[j] = BitConverter.ToUInt16(myBuffer, Icmp_header_buffer_index);
                        Icmp_header_buffer_index += 2;
                    }
                    //保存校验和
                    packet.CheckSum = IcmpPacket.SumOfCheck(Cksum_buffer);
                    //将报文转化为数据包
                    Byte[] Senddata = new Byte[Packetsize];
                    index = packet.CountByte(Senddata);
                    //报文出错
                    if (index != Packetsize)
                    {
                        //exception.Text = "报文出错2";
                        //backData.Add("报文出现问题", "0");
                        IcmpReturn information = new IcmpReturn();
                        information.Color = "0";//错误
                        string backJson = JsonConvert.SerializeObject(information);
                        backData.Add("1", backJson);
                        return backData;
                    }
                    int Nbytes = 0;
                    //系统计时
                    int starttime = Environment.TickCount;
                    //发送数据包
                    try
                    {
                        Nbytes = socket.SendTo(Senddata, Packetsize, SocketFlags.None, hostEndpoint);

                        if (Nbytes == -1)
                        {
                            //exception.Text = "无法传送";
                            //backData.Add("访问被拒绝", "403");
                            IcmpReturn information = new IcmpReturn();
                            information.Color = "0";//错误
                            string backJson = JsonConvert.SerializeObject(information);
                            backData.Add(i.ToString(), backJson);
                            return backData;
                        }
                        Byte[] Recewivedata = new Byte[256];
                        Nbytes = 0;
                        int Timeout = 0;
                        int timeconsume = 0;
                        while (true)
                        {
                            //socket.Blocking = false;
                            // 这里设置站点超时判断
                            socket.ReceiveTimeout = 1000;
                            try
                            {
                                Nbytes = socket.ReceiveFrom(Recewivedata, 256, SocketFlags.None, ref hostEndpoint);
                            }
                            catch (Exception e)
                            {
                                //服务器连接失败一类的异常
                                Nbytes = -1;
                                Debug.WriteLine(e.ToString());
                                DBHelper.InsertErrorLog(e.InnerException);
                            }

                            if (Nbytes == -1)
                            {
                                //exception.Text = "主机未响应";
                                //backData.Add("主机未响应", "404");
                                IcmpReturn information = new IcmpReturn();
                                information.Color = "0";//错误
                                string backJson = JsonConvert.SerializeObject(information);
                                backData.Add(i.ToString(), backJson);
                                //return backData;
                                break;
                            }
                            else if (Nbytes > 0)
                            {
                                timeconsume = System.Environment.TickCount - starttime;
                                //得到与发送间隔时间
                                //exception.Text += "reply from: " + hostep4.ToString() + "  In " + timeconsume + "ms:  bytes Received" + Nbytes + "\r\n";
                                IcmpReturn information = new IcmpReturn();
                                information.Time = timeconsume.ToString();
                                information.TTL = socket.Ttl.ToString();
                                information.Bytes = Nbytes.ToString();
                                information.Color = "1";
                                string backJson = JsonConvert.SerializeObject(information);
                                backData.Add(i.ToString(), backJson);
                                break;
                            }
                            timeconsume = Environment.TickCount - starttime;
                            if (Timeout > 1000)
                            {
                                //exception.Text = "time out";
                                //backData.Add("超时", "404");
                                IcmpReturn information = new IcmpReturn();
                                information.Color = "-1";//超时
                                string backJson = JsonConvert.SerializeObject(information);
                                backData.Add(i.ToString(), backJson);
                                //return backData;
                                break;
                            }
                        }
                        socket.Dispose();
                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                        IcmpReturn information = new IcmpReturn();
                        information.Color = "0";//超时
                        string backJson = JsonConvert.SerializeObject(information);
                        backData.Add(i.ToString(), backJson);
                        return backData;
                    }
                }
                return backData;
            }
            else
            {
                //backData.Add("ip地址不规范", "-1");
                IcmpReturn information = new IcmpReturn();
                information.Color = "0";//错误
                string backJson = JsonConvert.SerializeObject(information);
                backData.Add("1", backJson);
                return backData;
            }
        }

        /// <summary>
        /// Socket请求对应终端点
        /// </summary>
        /// <param name="desEndPoing">待请求终结点</param>
        /// <returns>Tuple-> 请求状态、请求耗时、请求后对应的站点颜色、请求的附带信息 </returns>
        public static async Task<Tuple<string, string, string, string>> SocketRequest(IPEndPoint desEndPoing)
        {
            // 请求状态
            string requestStatus = "";
            // 请求耗时
            string requestCost = "";
            // 请求结果
            string color = "";
            // 请求的额外信息
            string others = "";

            // 用来记录请求耗时
            var s = new System.Diagnostics.Stopwatch();
            s.Start();

            try
            {
                using (var TcpClient = new StreamSocket())
                {
                    // 超时控制
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(1000);

                    // HostName 构造需要一个主机名orIP地址(不带http!)
                    // 异步建立连接
                    await TcpClient.ConnectAsync(
                        new Windows.Networking.HostName(desEndPoing.Address.ToString()),
                        desEndPoing.Port.ToString(),
                        SocketProtectionLevel.PlainSocket)
                        // 作为Task任务，添加超时令牌
                        .AsTask(cts.Token);
                    // 停表
                    s.Stop();
                    var remoteIp = TcpClient.Information.RemoteAddress;
                    Debug.WriteLine(String.Format("Success, remote server contacted at IP address {0},and the connecting work cost {1} millsseconds!",
                                                                 remoteIp, s.ElapsedMilliseconds));
                    #region 修改返回数据
                    requestStatus = "200";
                    requestCost = s.ElapsedMilliseconds.ToString();
                    color = "green";
                    others = String.Format("Success, remote server contacted at IP address {0},and the connecting work cost {1} millsseconds!",
                                                                 remoteIp, s.ElapsedMilliseconds);
                    #endregion
                    // 释放连接
                    TcpClient.Dispose();
                    return new Tuple<string, string, string, string>(requestStatus, requestCost, color, others);
                }
            }
            // 捕获自定义超时异常
            catch (TaskCanceledException e)
            {
                #region 修改返回数据
                requestStatus = "1000";
                requestCost = s.ElapsedMilliseconds.ToString();
                color = "orange";
                others = "Error: Timeout when connecting (check hostname and port)";
                #endregion
                DBHelper.InsertErrorLog(e);
                return new Tuple<string, string, string, string>(requestStatus, requestCost, color, others);
            }
            // 捕获常见的异常
            catch (Exception ex)
            {
                s.Stop();
                // 查不到对应HostName的服务器
                if (ex.HResult == -2147013895)
                {
                    #region 修改返回数据
                    requestStatus = "0";
                    requestCost = s.ElapsedMilliseconds.ToString();
                    color = "red";
                    others = "Error: No such host is known";
                    #endregion
                    Debug.WriteLine("Error: No such host is known");
                    DBHelper.InsertErrorLog(ex);
                }
                // 请求超时
                else if (ex.HResult == -2147014836)
                {
                    #region 修改返回数据
                    requestStatus = "1000";
                    requestCost = s.ElapsedMilliseconds.ToString();
                    color = "orange";
                    others = "Error: Timeout when connecting (check hostname and port)";
                    #endregion
                    Debug.WriteLine("Error: Timeout when connecting (check hostname and port)");
                    DBHelper.InsertErrorLog(ex);
                }
                // 其他异常
                else
                {
                    #region 修改返回数据
                    requestStatus = "0";
                    requestCost = s.ElapsedMilliseconds.ToString();
                    color = "red";
                    others = "Error: Exception returned from network stack: " + ex.Message;
                    #endregion
                    Debug.WriteLine("Error: Exception returned from network stack: " + ex.Message);
                    DBHelper.InsertErrorLog(ex);
                }
                return new Tuple<string, string, string, string>(requestStatus, requestCost, color, others);
            }
        }

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
                HttpClientHandler handler = new HttpClientHandler();
                // 设置程序是否跟随重定向
                handler.AllowAutoRedirect = false;
                // 生成自定义的请求处理器
                CustomHandler cu = new CustomHandler();
                cu.InnerHandler = handler;
                HttpClient client = new HttpClient(cu);
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
    }
    class IcmpReturn
    {
        private string _ttl;
        public string TTL
        {
            get
            {
                return _ttl;
            }
            set
            {
                _ttl = value;
            }
        }

        private string _time;
        public string Time
        {
            get { return _time; }
            set { _time = value; }
        }

        private string _bytes;
        public string Bytes
        {
            get { return _bytes; }
            set { _bytes = value; }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set { _color = value; }
        }


    }
    class HttpReturn
    {

        private string _statusCode;

        public string StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }
        private int _requestTime;

        public int RequestTime
        {
            get { return _requestTime; }
            set { _requestTime = value; }
        }
        private string _color;

        public string Color
        {
            get { return _color; }
            set { _color = value; }
        }

    }
}
