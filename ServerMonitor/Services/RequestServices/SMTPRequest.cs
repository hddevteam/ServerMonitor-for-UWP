using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ServerMonitor.Services.RequestServices
{
    public class SMTPRequest : BasicRequest,IRequest
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
        /// 待请求的SMTP服务器的端口
        /// </summary>
        int port;
        public int Port { get => port; set => port = value; }

        /// <summary>
        /// 生成一个SMTP请求对象
        /// </summary>
        /// <param name="DomainName">待请求的SMTP域名</param>
        /// <param name="port">待请求的SMTP服务器的端口</param>
        public SMTPRequest(string DomainName,int port)
        {
            this.DomainName = DomainName;
            this.Port = port;
        }

        /// <summary>
        /// SMTP请求
        /// </summary>
        /// <returns>是否请求成功</returns>
        public async Task<bool> MakeRequest()
        {            
            CreateTime = DateTime.Now;// 赋值生成请求的时间
            try
            {
                // get all the ip with the domain  一般只有一个
                IPHostEntry hostInfo = await Dns.GetHostEntryAsync(DomainName);
                IPAddress[] IPaddresses = hostInfo.AddressList;
                IPAddress hostAddress = IPaddresses[0];// 主机IP地址
                IPEndPoint hostEndPoint = new IPEndPoint(hostAddress, port);// get our end point主机端点 IP地址+端口
                //用他来建立连接，发送信息 prepare the socket
                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Stopwatch stopwatch = new Stopwatch(); // 记录请求耗时
                CancellationTokenSource cts = new CancellationTokenSource();
                stopwatch.Start();
                // 二次封装任务，目的在于让请求过程变成可控的
                Task queryTask = Task.Run(async () =>
                {
                    await s.ConnectAsync(hostEndPoint);
                    //接收建立连接时的返回信息 port不对时，会超时接收
                    s.Receive(RecvFullMessage);//交代自己认证SMTP服务器的域名 然后发送 接收信息存在RecvFullMessage
                }, cts.Token);
                // 开启另一个任务同时进行用于记录是否超时
                var ranTask = Task.WaitAny(queryTask, Task.Delay(OverTime));
                if (0 != ranTask)
                {
                    s.Dispose();     //让连接暂停
                    // 取消任务，并返回超时异常
                    cts.Cancel();
                    stopwatch.Stop();
                    TimeCost = OverTime;
                    Status = "1002";
                    ErrorException = new TaskCanceledException("Overtime");
                    return false;
                }
                await Task.CompletedTask;
                stopwatch.Stop();

                if (s.Connected && queryTask.IsCompleted) // 请求成功，获取到了解析结果
                {
                    stopwatch.Start();  //开始计时
                    ByteCommand = ASCII.GetBytes("HELO " + DomainName + "\r\n"); //规定的HELO请求格式
                    s.Send(ByteCommand, ByteCommand.Length, 0);
                    s.Receive(RecvFullMessage);  //接收HELO请求的返回信息
                    stopwatch.Stop();   //结束计时

                    string str = ASCII.GetString(RecvFullMessage).Substring(0, 1); //去状态码
                    if (str.Equals("2"))
                    {
                        // Dns服务器状态良好
                        Status = "1000";
                        // 请求耗时应该在2^15-1(ms)内完成
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;         //计算请求时间
                        ActualResult = ASCII.GetString(RecvFullMessage);         //记录请求结果
                        return true;                                            //停止循环，返回true
                    }
                    else
                    {
                        // Dns服务器状态未知，但是该域名无法解析
                        Status = "1001";
                        // 请求耗时应该在2^15-1(ms)内完成
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;

                        return false;
                    }
                }
                else // 请求失败，无解析结果
                {
                    // Dns服务器状态未知，但是该域名无法解析
                    Status = "1001";
                    // 请求耗时应该在2^15-1(ms)内完成
                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
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
                return false;
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
