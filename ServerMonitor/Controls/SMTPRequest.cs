using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ServerMonitor.Controls
{
    class SMTPRequest : BasicRequest
    {
        Encoding ASCII = Encoding.ASCII;  //用来转码
        Byte[] ByteCommand;  //待发送命令
        Byte[] RecvFullMessage = new Byte[256];  //收到的链接信息
        /// <summary>
        /// 测试服务器状态使用的域名
        /// </summary>
        string domainName;
        public string DomainName { get => domainName; set => domainName = value; }
        /// <summary>
        /// 测试期待值
        /// </summary>
        string actualResult;
        public string ActualResult { get => actualResult; set => actualResult = value; }

        public SMTPRequest(string DomainName)
        {
            this.DomainName = DomainName;
        }

        /// <summary>
        /// SMTP请求
        /// </summary>
        /// <returns></returns>
        public override async Task<bool> MakeRequest()
        {
            CreateTime = DateTime.Now;
            try
            {
                Socket s = null;  //用他来建立连接，发送信息
                IPAddress hostAddress = null;  // 主机IP地址
                IPEndPoint hostEndPoint;     //主机端点 IP地址+端口
                // get all the ip with the domain
                IPHostEntry hostInfo = await Dns.GetHostEntryAsync(DomainName);
                IPAddress[] IPaddresses = hostInfo.AddressList;

                // go through each ip and attempt a connection
                for (int index = 0; index < IPaddresses.Length; index++)
                {
                    hostAddress = IPaddresses[index];
                    hostEndPoint = new IPEndPoint(hostAddress, 587);// get our end point
                    // prepare the socket
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    Stopwatch stopwatch = new Stopwatch(); // 记录请求耗时

                    DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, OverTime) };
                    timer.Tick += new EventHandler<object>((sender, e) =>
                    {
                        if (!s.Connected) // //Connection timed out...
                        {
                            Status = "1003";
                            TimeCost = OverTime;
                            ErrorException = new Exception("请求超时");
                            s.Dispose();
                            s = null;
                        }
                        timer.Stop();
                    });
                    
                    try
                    {
                        timer.Start();
                        await s.ConnectAsync(hostEndPoint);
                    }
                    catch (Exception e) //Connection timed out...
                    {
                        if (s != null)
                        {
                            Status = "1003";
                            ErrorException = e;
                            TimeCost = OverTime;
                        }
                        continue;
                    }
                    if (!s.Connected) // Connection failed, try next IPaddress. 当作超时处理
                    {
                        Status = "1003";
                        TimeCost = OverTime;
                        ErrorException = new Exception("未建立连接");
                    }
                    else
                    {
                        s.Receive(RecvFullMessage);//接受建立连接时的返回信息
                        //交代自己认证SMTP服务器的域名 然后发送 接收信息存在RecvFullMessage

                        stopwatch.Start();
                        ByteCommand = ASCII.GetBytes("HELO " + DomainName + "\r\n");
                        s.Send(ByteCommand, ByteCommand.Length, 0);
                        s.Receive(RecvFullMessage);
                        stopwatch.Stop();

                        Status = ASCII.GetString(RecvFullMessage).Substring(0, 3);
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;
                        ActualResult = ASCII.GetString(RecvFullMessage);
                        return true;
                    }

                }
            }
            catch (Exception e)
            {
                // 服务器超时
                Status = "1003";
                // 收集捕获到的异常
                ErrorException = e;
                // 请求耗时设置为超时上限
                TimeCost = OverTime;
            }
            return false;
        }
    }
}
