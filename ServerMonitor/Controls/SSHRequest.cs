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

        public string iPAddress { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }
        
        private bool overtime = false;

        public string ProtocolInfo { get; set; }
        
        public SSHRequest(string ipAddress,string username,string password)
        {
            this.iPAddress = ipAddress;
            this.UserName = username;
            this.PassWord = password;
        }
        
        public override async Task<bool> MakeRequest()
        {
            await Task.CompletedTask;
            //if (iPAddress == null || UserName == null || PassWord == null)
            //{
            //    throw new ArgumentNullException("one or more parameter is empty");
            //}
            // 赋值生成请求的时间
            CreateTime = DateTime.Now;
            var cSSH = new SshClient(iPAddress, 22, UserName, PassWord);

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
}
