using GalaSoft.MvvmLight.Threading;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ServerMonitor.Controls
{
    public class SSHRequest : BasicRequest
    {
        // 服务器IP地址
        private string ipAddress;

        public string iPAddress
        {
            get { return ipAddress; }
            set { ipAddress = value; }
        }

        //ssh用户名
        private string username;

        public string UserName
        {
            get { return username; }
            set { username = value; }
        }

        //ssh用户密码
        private string password;

        public string PassWord
        {
            get { return password; }
            set { password = value; }
        }
        private bool overtime = false;

        private string protocolInfo;

        public string ProtocolInfo
        {
            get { return protocolInfo; }
            set { protocolInfo = value; }
        }

        public SSHRequest(string ipAddress,string username,string password)
        {
            this.iPAddress = ipAddress;
            this.username = username;
            this.password = password;
        }
        
        public override async Task<bool> MakeRequest()
        {
            var con_result = await SSHConnectAsync();
            var result = await GetRequestResult(con_result);
            return result;
        }

        public async Task<Tuple<Tuple<Exception, SocketException, int>, short>> SSHConnectAsync()
        {
            // 赋值生成请求的时间
            CreateTime = DateTime.Now;
            var cSSH = new SshClient(iPAddress, 22, username, password);
            // 二次封装任务
            Task<Tuple<Exception, SocketException, int>> t = Task.Run(() =>
            {
                try
                {
                    cSSH.Connect();
                }
                catch (SshAuthenticationException e)
                {
                    throw e;
                    //return new Tuple<Exception, SocketException, int>(e, null, 1);
                }
                catch (SshOperationTimeoutException e)
                {
                    throw e;
                    return new Tuple<Exception, SocketException, int>(e, null, 2);
                }
                catch (SocketException e)
                {
                    throw e;
                    return new Tuple<Exception, SocketException, int>(null, e, 3);
                }
                catch (SshConnectionException e)
                {
                    throw e;
                    //return new Tuple<Exception, SocketException, int>(e, null, 4);
                }
                catch (Exception e)
                {
                    throw e;
                    //return new Tuple<Exception, SocketException, int>(e, null, 5);
                }
                return new Tuple<Exception, SocketException, int>(null, null, 0);
            });
            // 记录请求耗时
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await t;
            stopwatch.Stop();
            cSSH.Disconnect();
            return new Tuple<Tuple<Exception, SocketException, int>, short>(result,(short)stopwatch.ElapsedMilliseconds);
        }

        public async Task<bool> GetRequestResult(Tuple<Tuple<Exception, SocketException, int>, short> tuple)
        {
            await Task.CompletedTask;
            var result = tuple.Item1;
            var timeCost = tuple.Item2;
            //若无异常，则认为连接成功
            if (result.Item1 == result.Item2 )
            {
                Debug.WriteLine("Connected true.");
                Status = "1000";
                TimeCost = timeCost;
                return true;
            }
            else
            {
                // 服务器连接失败（各种原因）
                switch (result.Item3)
                {
                    case 1:
                        ProtocolInfo = "用户名或密码错误"; break;
                    case 2:
                        ProtocolInfo = "连接超时";
                        Status = "1002";
                        TimeCost = OverTime; break;
                    case 3:
                        ProtocolInfo = "(SocketException)" +
                            result.Item2.SocketErrorCode.ToString(); break;
                    case 4:
                        ProtocolInfo = "响应不包含SSH协议标识"; break;
                    case 5:
                        ProtocolInfo = "其他错误"; break;
                    default:
                        break;
                }
                //Debug.WriteLine("Connected false.+异常：" + result.Item1.Message);
                //非超时错误
                if (Status == null)
                {
                    Status = "1001";
                    TimeCost = timeCost;
                }
                // 收集捕获到的异常
                ErrorException = result.Item1;
                
                return false;
            }
        }
    }
}
