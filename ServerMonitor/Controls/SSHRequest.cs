using GalaSoft.MvvmLight.Threading;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
        /// 服务器IP地址
        /// </summary>
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

        public SSHRequest(string ipAddress,string username,string password)
        {
            this.iPAddress = ipAddress;
            this.username = username;
            this.password = password;
        }
        
        public override async Task<bool> MakeRequest()
        {
            // 赋值生成请求的时间
            CreateTime = DateTime.Now;
            using (var cSSH = new SshClient(iPAddress, 22, username, password))
            {
                // 超时控制
                CancellationTokenSource cts = new CancellationTokenSource();
                cts.CancelAfter(OverTime);
                try
                {
                    // 记录请求耗时
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    
                    //倒计时1000ms
                    DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 0, 0, OverTime) };   
                    timer.Start();
                    // 二次封装任务
                    Task<Exception> t = Task.Run(async() => 
                    {
                        var t1 = Task<Exception>.Factory.StartNew(() => {
                            try
                            {
                                cSSH.Connect();
                            }
                            catch (ObjectDisposedException e)
                            {
                                Debug.WriteLine("e1:" + e);
                                return e;
                            }
                            catch (InvalidOperationException e)
                            {
                                Debug.WriteLine("e2:" + e);
                                return e;
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("e3:" + e);
                                return e;
                            }
                            return null;
                        });

                        while (!t1.IsCompleted)
                        {
                            if (cts.Token.IsCancellationRequested)
                            {
                                Debug.WriteLine("强制取消");
                                cts.Token.ThrowIfCancellationRequested(); 
                            }
                        }
                        return await t1;
                    }, cts.Token);
                    timer.Tick += new EventHandler<object>((sender, e) =>
                    {
                        if (!t.IsCompleted)
                        {
                            cts.Cancel();
                        }
                        timer.Stop();
                    });
                    var result = await t;
                    stopwatch.Stop();
                    
                    if (t.IsCompleted&&cSSH.IsConnected)
                    {
                        Debug.WriteLine("Connected true.");
                        Status = "1000";
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Connected false.+异常：" + result.Message);
                        // 服务器连接失败（地址/用户名/密码有误）
                        Status = "1002";
                        // 收集捕获到的异常
                        ErrorException = result;
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;
                        return false;
                    }
                }
                // 捕获到请求超时的情况(主机ip未开启SSH或不存在都会引起超时)
                catch (OperationCanceledException e)
                {
                    // 服务器超时
                    Status = "1002";
                    // 收集捕获到的异常
                    ErrorException = e;
                    // 请求耗时设置为超时上限
                    TimeCost = OverTime;
                    return false;
                }
                finally
                {
                    cSSH.Disconnect();
                }
            }
            
        }
    }
}
