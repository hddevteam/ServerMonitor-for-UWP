using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerMonitor.Services.RequestServices
{
    /**
     * Ftp 请求模块
     * 
     * 没有做成单例模式的是因为考虑到有场景会需要多个request对象
     */
    public class FTPRequest : BasicRequest,IRequest
    {
        // 继承的属性：CreateTime TimeCost OverTime Status Others ErrorException               
        /// <summary>
        /// Ftp控制进程端口
        /// </summary>
        private static short port = 21;
        /// <summary>
        /// 服务器返回信息
        /// </summary>
        private string protocalInfo = null;
        /// <summary>
        /// Ftp登入类型（匿名|身份验证）
        /// </summary>
        private LoginType identifyType = LoginType.Identify;
        /// <summary>
        /// Ftp登入信息
        /// </summary>
        private IdentificationInfo identification = new IdentificationInfo();
        /// <summary>
        /// Ftp 服务器地址
        /// </summary>
        private IPAddress ftpServer;
        /// <summary>
        /// 登入命令
        /// </summary>
        private string USERCOMMAND = "USER {0}\r\n";
        /// <summary>
        /// 密码命令
        /// </summary>
        private string PASSCOMMAND = "PASS {0}\r\n";
        /// <summary>
        /// 接受传回内容的的Buffer
        /// </summary>
        private byte[] RecvBuffer = new byte[256];

        // 将协议内容设置成只读的，旨在提高封装程度，去除外部的入口
        public string ProtocalInfo { get => protocalInfo; }
        //public LoginType IdentifyType { get => identifyType; set => identifyType = value; }
        public IdentificationInfo Identification { get => identification; set => identification = value; }
        public IPAddress FtpServer { get => ftpServer; set => ftpServer = value; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="server">指定的ftp服务器</param>
        /// <param name="type">指定的登入类型</param>
        public FTPRequest(LoginType type)
        {
            identifyType = type;
            // 选择匿名登入的时候默认设置用户名为anonymous
            if (LoginType.Anonymous.Equals(type))
            {
                Identification.Username = "anonymous";
            }

        }

        // 需要自己手动加入一些垃圾回收的代码，以防内存泄漏（断开连接，释放连接）
        /// <summary>
        /// 对指定的IPAddress发起登入请求
        /// </summary>
        /// <returns>true:成功|false:失败</returns>
        public async Task<bool> MakeRequest()
        {
            // 定义用于存放ftp指令的byte数组
            byte[] PASSBytes = null;
            byte[] USERBytes = null;
            // 针对登入方式不同进行不同的验证信息复制
            switch (identifyType)
            {
                case LoginType.Anonymous:
                    USERBytes = SendUSERCommand("anonymous");
                    PASSBytes = string.IsNullOrEmpty(Identification.Password) ? null : SendPASSCommand(Identification.Password);
                    break;
                case LoginType.Identify:
                    USERBytes = string.IsNullOrEmpty(Identification.Username) ? null : SendUSERCommand(Identification.Username);
                    PASSBytes = string.IsNullOrEmpty(Identification.Password) ? null : SendPASSCommand(Identification.Password);
                    break;
                default:
                    // ftp请求登陆模式异常
                    CreateTime = DateTime.Now;
                    TimeCost = 0;
                    Status = "1001";
                    ErrorException = new Exception("User identifyType is invalid!");
                    protocalInfo = "User identifyType is invalid!";
                    return false;
            }

            // 填入CreateTime
            CreateTime = DateTime.Now;

            
            // 检测服务器的IPAddress是否合法
            if (! ValidateIPv4(ftpServer.ToString()))
            {
                TimeCost = 0;
                Status = "1001";
                ErrorException = new Exception("Server address is invalid!");
                protocalInfo = "Server address is invalid!";
                throw new ArgumentNullException("Server address is invalid empty!");
            }

            // 建立的Socket Ipv4的协议 采用的面向连接的方式 协议类型 Tcp
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                // 需要添加超时判断
                socket.ReceiveTimeout = OverTime;
                // 记录此次请求耗时的计时器
                Stopwatch stopwatch = new Stopwatch();
                // 用于取消请求任务
                CancellationTokenSource cts = new CancellationTokenSource();
                try
                {
                    stopwatch.Start();
                    // 建立远程连接
                    Task connectTask = Task.Run(async() =>
                    {
                        await socket.ConnectAsync(new IPEndPoint(ftpServer, port));

                        // 获取连接状态
                        if (socket.Connected)
                        {
                            // 存放接受的信息的长度
                            Int32 bytes = 0;
                            // 存放接受信息内容
                            string strRet = "";

                            // 获取建立连接的返回信息
                            bytes = socket.Receive(RecvBuffer, RecvBuffer.Length, 0);
                            strRet = Encoding.ASCII.GetString(RecvBuffer, 0, bytes);

                            // 登录指令
                            socket.Send(USERBytes, USERBytes.Length, 0);
                            // 获取返回信息
                            bytes = socket.Receive(RecvBuffer, RecvBuffer.Length, 0);
                            strRet = Encoding.ASCII.GetString(RecvBuffer, 0, bytes);

                            // 密码指令
                            socket.Send(PASSBytes, PASSBytes.Length, 0);
                            // 获取返回信息
                            bytes = socket.Receive(RecvBuffer, RecvBuffer.Length, 0);
                            strRet = Encoding.ASCII.GetString(RecvBuffer, 0, bytes);

                            // 停表
                            stopwatch.Stop();

                            // 截取获得返回状态码
                            int commandcode = int.Parse(strRet.Substring(0, 1));
                            // 230 530 为常见返回值，其中230为成功 530为登入失败
                            switch (commandcode)
                            {
                                // 请求成功
                                case 2:
                                case 3:
                                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                    Status = "1000";
                                    protocalInfo = strRet;
                                    break;
                                // 请求失败 
                                case 4:
                                case 5:
                                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                    Status = "1001";
                                    protocalInfo = strRet;
                                    break;
                                // 请求未知
                                default:
                                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                    Status = "1001";
                                    protocalInfo = strRet;
                                    break;
                            }
                        }
                        else
                        {
                            // 未连接，将秒表停止，此次请求作为失败的请求
                            stopwatch.Stop();
                            TimeCost = (short)stopwatch.ElapsedMilliseconds;
                            Status = "1001";
                            protocalInfo = "Connect failed!";
                        }
                    },cts.Token);
                    // 开启另一个任务同时进行用于记录是否超时
                    var ranTask = Task.WaitAny(connectTask, Task.Delay(OverTime));
                    if (0 != ranTask)
                    {
                        // 取消任务，并返回超时异常
                        cts.Cancel();
                        
                        stopwatch.Stop();
                        TimeCost = OverTime;
                        Status = "1002";
                        Exception e = new Exception("Overtime");
                        protocalInfo = e.ToString();
                        ErrorException = e;                        
                    }
                    await Task.CompletedTask;
                }
                catch (FormatException e)
                {
                    // 输入的指令格式不正确 ，此次请求作为失败的请求
                    stopwatch.Stop();
                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                    Status = "1001";
                    protocalInfo = e.ToString();
                    ErrorException = e;
                }                
                catch (SocketException e)
                {
                    stopwatch.Stop();
                    // 连接建立出现问题 ，此次请求作为失败的请求
                    switch (e.HResult)
                    {
                        case -2147467259:
                            stopwatch.Stop();
                            TimeCost = OverTime;
                            Status = "1002";
                            protocalInfo = e.ToString();
                            ErrorException = e;
                            break;
                        default:
                            TimeCost = (short)stopwatch.ElapsedMilliseconds;
                            Status = "1001";
                            protocalInfo = e.Message;
                            break;
                    }
                    ErrorException = e;
                }
                catch (Exception e)
                {
                    // 出现未捕获到的异常，此次请求作为失败的请求
                    stopwatch.Stop();
                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                    Status = "1001";
                    protocalInfo = e.ToString();
                    ErrorException = e;
                }

                // 决定此次请求成功/失败
                if ("1000".Equals(Status))
                {
                    return true;
                }
                else
                {
                    if (string.IsNullOrEmpty(protocalInfo))
                    {
                        protocalInfo = "Access Failed";
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// ftp用户名命令
        /// </summary>
        /// <param name="Username"></param>
        /// <returns></returns>
        private byte[] SendUSERCommand(string Username)
        {
            return string.IsNullOrEmpty(Username) ? null : Encoding.ASCII.GetBytes(string.Format(USERCOMMAND, Username));
        }

        /// <summary>
        /// ftp密码命令
        /// </summary>
        /// <param name="Password"></param>
        /// <returns></returns>
        private byte[] SendPASSCommand(string Password)
        {
            return string.IsNullOrEmpty(Password) ? null : Encoding.ASCII.GetBytes(string.Format(PASSCOMMAND, Password));
        }

        /// <summary>
        ///  用于判断ip是否合法
        /// </summary>
        /// <param name="ipString">IPAddress</param>
        /// <returns>yes:true|no:false</returns>
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            return splitValues.All(r => byte.TryParse(r, out byte tempForParsing));
        }
    }

    /// <summary>
    /// 用户身份验别信息
    /// </summary>
    public class IdentificationInfo
    {
        public IdentificationInfo()
        {
            Username = "";
            Password = "";
            Email = "";
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username;
        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password;
        /// <summary>
        /// 用户邮箱
        /// </summary>
        public string Email;
    }

    /// <summary>
    /// 用户登入识别类型
    /// </summary>
    public enum LoginType
    {
        /// <summary>
        /// 匿名登入
        /// </summary>
        Anonymous,
        /// <summary>
        /// 特定用户登入
        /// </summary>
        Identify
    };

}
