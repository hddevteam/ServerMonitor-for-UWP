using ServerMonitor.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.Sockets;

namespace ServerMonitor.Services.RequestServices
{
    public class SocketRequest : BasicRequest, IRequest
    {
        // 继承的属性：CreateTime TimeCost OverTime Status Others ErrorException
        private IPEndPoint targetEndPoint;
        private string protocolInfo;
        /// <summary>
        /// 目标终端
        /// </summary>
        public IPEndPoint TargetEndPoint { get => targetEndPoint; set => targetEndPoint = value; }
        public string ProtocolInfo { get => protocolInfo;}

        public async Task<bool> MakeRequest()
        {
            CreateTime = DateTime.Now;
            // 用来记录请求耗时
            var s = new System.Diagnostics.Stopwatch();
            s.Start();

            try
            {
                using (var TcpClient = new StreamSocket())
                {
                    // 超时控制
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(OverTime);

                    // HostName 构造需要一个主机名orIP地址(不带http!)
                    // 异步建立连接
                    await TcpClient.ConnectAsync(
                        new Windows.Networking.HostName(targetEndPoint.Address.ToString()),
                        targetEndPoint.Port.ToString(),
                        SocketProtectionLevel.PlainSocket)
                        // 作为Task任务，添加超时令牌
                        .AsTask(cts.Token);
                    // 停表
                    s.Stop();
                    var remoteIp = TcpClient.Information.RemoteAddress;
                    Debug.WriteLine(String.Format("Success, remote server contacted at IP address {0},and the connecting work cost {1} millsseconds!",
                                                                 remoteIp, s.ElapsedMilliseconds));
                    #region 修改返回数据
                    Status = "1000";
                    TimeCost = (short)s.ElapsedMilliseconds;
                    protocolInfo = String.Format("Success, remote server contacted at IP address {0},and the connecting work cost {1} millsseconds!", remoteIp, s.ElapsedMilliseconds);
                    #endregion
                    // 释放连接
                    TcpClient.Dispose();
                }
                return true;
            }
            // 捕获自定义超时异常
            catch (TaskCanceledException e)
            {
                #region 修改返回数据
                Status = "1002";
                TimeCost = OverTime;
                protocolInfo = "Error: Timeout when connecting (check hostname and port)";
                #endregion
                DBHelper.InsertErrorLog(e);
                return false;
            }
            // 捕获常见的异常
            catch (Exception ex)
            {
                s.Stop();
                // 查不到对应HostName的服务器
                if (ex.HResult == -2147013895)
                {
                    #region 修改返回数据
                    Status = "1001";
                    TimeCost = (short)(OverTime * 2);
                    protocolInfo = String.Format("Success, remote server contacted at IP address {0},and the connecting work cost {1} millsseconds!", targetEndPoint.Address, s.ElapsedMilliseconds);
                    #endregion
                    Debug.WriteLine("Error: No such host is known");
                    DBHelper.InsertErrorLog(ex);
                }
                // 请求超时
                else if (ex.HResult == -2147014836)
                {
                    #region 修改返回数据
                    Status = "1002";
                    TimeCost = OverTime;
                    protocolInfo = "Error: Timeout when connecting (check hostname and port)";
                    #endregion
                    Debug.WriteLine("Error: Timeout when connecting (check hostname and port)");
                    DBHelper.InsertErrorLog(ex);
                }
                // 其他异常
                else
                {
                    #region 修改返回数据
                    Status = "1001";
                    TimeCost = (short)(OverTime * 2);
                    protocolInfo = "Error: Timeout when connecting (check hostname and port)";
                    #endregion
                    Debug.WriteLine("Error: Timeout when connecting (check hostname and port)");
                    DBHelper.InsertErrorLog(ex);
                }
                return false;
            }
        }
    }
}
