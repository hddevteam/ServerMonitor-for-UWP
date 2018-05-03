using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                    // 二次封装任务
                    Task<Exception> t = Task.Run( () => 
                    {
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
                            Debug.WriteLine("e3:"+e);
                            return e;
                        }
                        return null;
                    }, cts.Token);
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
                        //连接失败（各种原因）
                        Debug.WriteLine("异常：" + result.Message);
                        Debug.WriteLine("Connected false.");
                        // 服务器连接失败（地址用户名密码有误）
                        Status = "1002";
                        // 收集捕获到的异常
                        ErrorException = result;
                        TimeCost = (short)stopwatch.ElapsedMilliseconds;
                        return false;
                    }
                }
                // 捕获到请求超时的情况
                catch (TaskCanceledException e)
                {
                    // 服务器超时
                    Status = "1002";
                    // 收集捕获到的异常
                    ErrorException = e;
                    // 请求耗时设置为超时上限
                    TimeCost = OverTime;
                    return false;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
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
