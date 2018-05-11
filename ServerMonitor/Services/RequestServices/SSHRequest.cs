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
        /// <summary>
        /// Ssh端口
        /// </summary>
        private static short port = 22;

        public string IPAddress { get; set; }

        /// <summary>
        /// Ssh登入类型（匿名|身份验证）
        /// </summary>
        private SshLoginType identifyType = SshLoginType.Identify;
        /// <summary>
        /// Ssh登入信息
        /// </summary>
        private SshIdentificationInfo identification = new SshIdentificationInfo();

        public string ProtocolInfo { get; set; }
        public SshIdentificationInfo Identification { get => identification; set => identification = value; }

        public SSHRequest(string ipAddress,SshLoginType type)
        {
            this.IPAddress = ipAddress;
            identifyType = type;
            // 选择匿名登入的时候默认设置用户名，密码为anonymous
            if (SshLoginType.Anonymous.Equals(type))
            {
                Identification.Username = "anonymous";
                Identification.Password = "anonymous";
            }
        }
        
        public override async Task<bool> MakeRequest()
        {
            await Task.CompletedTask;
            // 赋值生成请求的时间
            CreateTime = DateTime.Now;
            var cSSH = new SshClient(IPAddress, port, Identification.Username, Identification.Password);

            // 记录请求耗时
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                cSSH.Connect();
                if (cSSH.IsConnected)
                {
                    stopwatch.Stop();
                    Status = "1000";
                    TimeCost = (short)stopwatch.ElapsedMilliseconds;
                    return true;
                }
            }
            catch (SshAuthenticationException e)
            {
                stopwatch.Stop();
                ProtocolInfo = "Authentication Failed.";
                ErrorException = e;
            }
            catch (SshOperationTimeoutException e)
            {
                stopwatch.Stop();
                ProtocolInfo = "Connection timed out.";
                Status = "1002";
                TimeCost = OverTime;
                ErrorException = e;
                return false;
            }
            catch (SocketException e)
            {
                stopwatch.Stop();
                ProtocolInfo = "(SocketException)" + e.SocketErrorCode.ToString();
                ErrorException = e;
            }
            catch (SshConnectionException e)
            {
                stopwatch.Stop();
                ProtocolInfo = "Response does not include the SSH protocol ID.";
                ErrorException = e;
            }
            catch (Exception e)
            {
                stopwatch.Stop();
                ProtocolInfo = "Other errors.";
                ErrorException = e;
            }
            finally
            {
                cSSH.Disconnect();
            }
            Status = "1001";
            TimeCost = (short)stopwatch.ElapsedMilliseconds;
            return false;
        }
    }

    /// <summary>
    /// 用户身份验别信息
    /// </summary>
    public class SshIdentificationInfo
    {
        public SshIdentificationInfo()
        {
            Username = "";
            Password = "";
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username;
        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password;
    }

    /// <summary>
    /// 用户登入识别类型
    /// </summary>
    public enum SshLoginType
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
