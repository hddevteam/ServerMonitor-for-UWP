using Newtonsoft.Json;
using ServerMonitor.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
/// <summary>
/// last modification by wzp on 2018/5/26
/// ICMP相关方法
/// </summary>

namespace ServerMonitor.Services.RequestServices
{
    /// <summary>
    /// icmp请求方法
    /// wzp 2018/5/31
    /// </summary>
    public class ICMPRequest
    {
        public static Dictionary<string, string> backData = new Dictionary<string, string>();

        public ICMPRequest(IPAddress iPAddress)
        {
            //icmp构造函数
            this.MyIPAddress = iPAddress;
        }

        public bool MakeRequest()
        {
            //backData.Clear();
            if (MyIPAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                //传入是正确的Ipv4格式
                 EndPoint hostEndpoint = (EndPoint)new IPEndPoint(MyIPAddress, 1025);
                //循环5次发送icmp包的操作
                for (int i = 0; i < 5; i++)
                {
                    RequestObj request = new RequestObj
                    {
                        CreateTime = DateTime.Now
                    };//创建一个请求对象
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();//记录耗时
                    int Datasize = 4;
                    int Packetsize = 8 + Datasize;
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp);
                    EndPoint clientep = (EndPoint)new IPEndPoint(IPAddress.Parse("127.0.0.1"), 30);
                    IcmpPacket packet = new IcmpPacket(8, 0, 0, 45, 0, Datasize);
                    Byte[] myBuffer = new Byte[Packetsize];
                    int index = packet.CountByte(myBuffer);
                    if (index != Packetsize)
                    {
                        request.Color = "0";
                        request.Status = "1001";
                        request.TimeCost = (short)(request.OverTime * request.ErrorQuality);
                        Requests.Add(request);
                        return false;
                    }
                    int Cksum_buffer_length = (int)Math.Ceiling(((Double)index) / 2);
                    UInt16[] Cksum_buffer = new UInt16[Cksum_buffer_length];
                    int Icmp_header_buffer_index = 0;
                    for (int j = 0; j < Cksum_buffer_length; j++)
                    {
                        //把两个byte转化为一个uint16
                        Cksum_buffer[j] = BitConverter.ToUInt16(myBuffer, Icmp_header_buffer_index);
                        Icmp_header_buffer_index += 2;
                    }
                    //保存校验和
                    packet.CheckSum = IcmpPacket.SumOfCheck(Cksum_buffer);
                    //将报文转化为数据包
                    Byte[] Senddata = new Byte[Packetsize];
                    index = packet.CountByte(Senddata);
                    //报文出错
                    if (index != Packetsize)
                    {
                        request.Color = "0";
                        request.Status = "1001";
                        request.TimeCost = (short)(request.OverTime * request.ErrorQuality);
                        Requests.Add(request);
                        return false;
                    }
                    int Nbytes = 0;
                    //系统计时
                    int starttime = Environment.TickCount;
                    //发送数据包
                    try
                    {
                        Nbytes = socket.SendTo(Senddata, Packetsize, SocketFlags.None, hostEndpoint);

                        if (Nbytes == -1)
                        {
                            //exception.Text = "无法传送";
                            //backData.Add("访问被拒绝", "403");
                            //IcmpReturn information = new IcmpReturn
                            //{
                            //    Color = "0"//错误
                            //};
                            //string backJson = JsonConvert.SerializeObject(information);
                            //backData.Add(i.ToString(), backJson);
                            //Color = "0";//访问被拒绝
                            request.Color = "0";
                            request.Status = "1001";
                            request.TimeCost = (short)(request.OverTime * request.ErrorQuality);
                            //TimeCost = (short)stopwatch.ElapsedMilliseconds;
                            Requests.Add(request);
                            return false;
                        }
                        Byte[] Recewivedata = new Byte[256];
                        Nbytes = 0;
                        //int Timeout = 0;
                        int timeconsume = 0;
                        while (true)
                        {
                            //socket.Blocking = false;
                            // 这里设置站点超时判断
                            //socket.ReceiveTimeout = 1000;
                            socket.ReceiveTimeout = request.OverTime;
                            try
                            {
                                Nbytes = socket.ReceiveFrom(Recewivedata, 256, SocketFlags.None, ref hostEndpoint);
                                
                            }
                            catch (Exception e)
                            {
                                //服务器连接失败一类的异常
                                Nbytes = -1;
                                //Debug.WriteLine(e.ToString());
                                DBHelper.InsertErrorLog(e);
                                request.ErrorException = e;
                            }
                            EndPoint correctEndpoint = (EndPoint)new IPEndPoint(MyIPAddress, 0);
                            if (hostEndpoint.GetHashCode() != correctEndpoint.GetHashCode())//对host取哈希判断是否是正确主机
                            {
                                //不是正确的主机进行回复
                                request.Color = "0";
                                request.Status = "1001";
                                request.TimeCost = (short)(request.OverTime * request.ErrorQuality);
                                Requests.Add(request);
                                break;
                            }
                            if (Nbytes == -1)
                            {
                                //主机未响应
                                request.Color = "0";
                                request.Status = "1001";
                                request.TimeCost = (short)(request.OverTime * request.ErrorQuality);
                                Requests.Add(request);
                                break;
                            }
                            else if (Nbytes > 0)
                            {
                                timeconsume = System.Environment.TickCount - starttime;                             
                                request.Color = "1";
                                request.Status = "1000";
                                request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                Requests.Add(request);
                                break;
                            }
                            timeconsume = Environment.TickCount - starttime;
                            if (timeconsume > 1000)
                            {
                                //超时
                                request.Color = "-1";
                                request.Status = "1002";
                                request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                Requests.Add(request);
                                break;
                            }
                        }
                        socket.Dispose();
                    }
                    catch (Exception ex)
                    {   //捕捉未知异常
                        string s = ex.Message;
                        DBHelper.InsertErrorLog(ex);
                        request.Color = "0";
                        request.Status = "1001";
                        request.TimeCost = (short)(request.OverTime * request.ErrorQuality);
                        request.ErrorException = ex;
                        Requests.Add(request);
                        return false;
                    }
                }
                return true;
            }
            else
            {
                RequestObj request = new RequestObj
                {
                    CreateTime = DateTime.Now
                };//创建一个请求对象
                request.Color = "0";
                request.Status = "1001";
                request.TimeCost = (short)(request.OverTime * request.ErrorQuality);       
                return false;
            }
        }

        public IPAddress MyIPAddress { get; set; }//请求的ip地址
        public List<RequestObj> Requests = new List<RequestObj>();
    }
    public class RequestObj :BasicRequest{
        //请求对象
        //一个请求对象包括  一次请求 的 创建时间、花费时间、请求结果等信息
        public String Color { get; set; }
    }
}
