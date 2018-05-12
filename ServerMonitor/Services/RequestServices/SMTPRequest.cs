using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ServerMonitor.Services.RequestServices
{
    class SMTPRequest : BasicRequest,IRequest
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

        /// <summary>
        /// 生成一个SMTP请求对象
        /// </summary>
        /// <param name="DomainName">待请求的SMTP域名</param>
        public SMTPRequest(string DomainName)
        {
            this.DomainName = DomainName;
        }

        /// <summary>
        /// SMTP请求
        /// </summary>
        /// <returns>是否请求成功</returns>
        public async Task<bool> MakeRequest()
        {
            CreateTime = DateTime.Now;// 赋值生成请求的时间
            bool outTime = false; //true 为超时
            try
            {
                Socket s = null;  //用他来建立连接，发送信息
                IPAddress hostAddress = null;  // 主机IP地址
                IPEndPoint hostEndPoint;     //主机端点 IP地址+端口
                // get all the ip with the domain  一般只有一个
                IPHostEntry hostInfo = await Dns.GetHostEntryAsync(DomainName);
                IPAddress[] IPaddresses = hostInfo.AddressList;

                // go through each ip and attempt a connection，成功请求就停止（返回true），失败就继续下一个（有的话）
                for (int index = 0; index < IPaddresses.Length; index++)
                {
                    outTime = false; //true 为超时  ，false为不超时
                    hostAddress = IPaddresses[index];
                    hostEndPoint = new IPEndPoint(hostAddress, 587);// get our end point
                    // prepare the socket
                    s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    Stopwatch stopwatch = new Stopwatch(); // 记录请求耗时

                    //捕捉连接超时，异步执行timer.Tick事件的方法，该方法只在OverTime（ms）后执行一次
                    DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, OverTime) };
                    timer.Tick += new EventHandler<object>((sender, e) =>
                    {
                        if (!s.Connected) // //Connection timed out...
                        {
                            Status = "1002"; //超时1002  错误1001
                            TimeCost = OverTime;    //设为规定值
                            ErrorException = new Exception("请求超时");  //记录错误信息
                            s.Dispose();     //让连接暂停
                            outTime = true;      // 标记为超时，且此情况已处理
                        }
                        timer.Stop();  //关闭计算器，让该方法只执行一次，
                    });

                    try
                    {
                        timer.Start();  //开始执行timer.Tick事件的方法
                        //建立TCP连接，不能用s.Connect(hostEndPoint); 会使timer.Tick无法正常执行
                        await s.ConnectAsync(hostEndPoint);  
                    }
                    catch (Exception e) //Connection error
                    {
                        if (outTime == false)  //false为不超时 ,对此情况下的报错处理
                        {
                            Status = "1001";
                            ErrorException = e;
                            TimeCost = OverTime;
                        }
                        continue;
                    }
                    if (!s.Connected) // Connection failed
                    {
                        Status = "1001";
                        TimeCost = OverTime;
                        ErrorException = new Exception("未建立连接");
                    }
                    else
                    {
                        s.Receive(RecvFullMessage);//接收建立连接时的返回信息
                        //交代自己认证SMTP服务器的域名 然后发送 接收信息存在RecvFullMessage

                        stopwatch.Start();  //开始计时
                        ByteCommand = ASCII.GetBytes("HELO " + DomainName + "\r\n"); //规定的HELO请求格式
                        s.Send(ByteCommand, ByteCommand.Length, 0);
                        s.Receive(RecvFullMessage);  //接收HELO请求的返回信息
                        stopwatch.Stop();   //结束计时

                        Status = ASCII.GetString(RecvFullMessage).Substring(0, 3); //去状态码
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;         //计算请求时间
                        ActualResult = ASCII.GetString(RecvFullMessage);         //记录请求结果
                        return true;                                            //停止循环，返回true
                    }
                }
            }
            catch (Exception e)
            {
                // 其他错误，例如域名解析错误
                Status = "1001";
                // 收集捕获到的异常
                ErrorException = e;
                // 请求耗时设置为超时上限
                TimeCost = OverTime;
            }
            return false;
        }
    }
}
