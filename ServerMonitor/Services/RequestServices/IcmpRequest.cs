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

namespace ServerMonitor.Services.RequestServices
{
    class ICMPRequest
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
                    //CreateTime = DateTime.Now;
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
                        //exception.Text = "报文出现问题";
                        //backData.Add("报文出现问题", "-1");
                        //IcmpReturn information = new IcmpReturn
                        //{
                        //    Color = "0"//错误
                        //};
                        //string backJson = JsonConvert.SerializeObject(information);
                        //backData.Add("1", backJson);
                        //Color = "0";
                        request.Color = "0";
                        request.Status = "1001";
                        request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
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
                        //exception.Text = "报文出错2";
                        //backData.Add("报文出现问题", "0");
                        //Color = "0";
                        request.Color = "0";
                        request.Status = "1001";
                        request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                        //string backJson = JsonConvert.SerializeObject(information);
                        //backData.Add("1", backJson);
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
                            request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
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
                                DBHelper.InsertErrorLog(e.InnerException);
                            }
                            EndPoint correctEndpoint = (EndPoint)new IPEndPoint(MyIPAddress, 0);
                            if (hostEndpoint != correctEndpoint)
                            {
                                //不是正确的主机进行回复
                                request.Color = "0";
                                request.Status = "1001";
                                //string backJson = JsonConvert.SerializeObject(information);
                                //backData.Add(i.ToString(), backJson);
                                //return backData;
                                request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                Requests.Add(request);
                                break;
                            }
                            if (Nbytes == -1)
                            {
                                //exception.Text = "主机未响应";
                                //backData.Add("主机未响应", "404");
                                //IcmpReturn information = new IcmpReturn
                                //{
                                //    Color = "0"//错误
                                //};
                                request.Color = "0";
                                request.Status = "1001";
                                //string backJson = JsonConvert.SerializeObject(information);
                                //backData.Add(i.ToString(), backJson);
                                //return backData;
                                request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                Requests.Add(request);
                                break;
                            }
                            else if (Nbytes > 0)
                            {
                                timeconsume = System.Environment.TickCount - starttime;
                                //得到与发送间隔时间
                                //exception.Text += "reply from: " + hostep4.ToString() + "  In " + timeconsume + "ms:  bytes Received" + Nbytes + "\r\n";
                                //IcmpReturn information = new IcmpReturn
                                //{
                                //    Time = timeconsume.ToString(),
                                //    TTL = socket.Ttl.ToString(),
                                //    Bytes = Nbytes.ToString(),
                                //    Color = "1"
                                //};
                                //string backJson = JsonConvert.SerializeObject(information);
                                //backData.Add(i.ToString(), backJson);
                                request.Color = "1";
                                request.Status = "1000";
                                request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                Requests.Add(request);
                                break;
                            }
                            timeconsume = Environment.TickCount - starttime;
                            if (timeconsume > 1000)
                            {
                                //exception.Text = "time out";
                                //backData.Add("超时", "404");
                                //IcmpReturn information = new IcmpReturn
                                //{
                                //    Color = "-1"//超时
                                //};
                                request.Color = "-1";
                                request.Status = "1002";
                                //string backJson = JsonConvert.SerializeObject(information);
                                //backData.Add(i.ToString(), backJson);
                                //return backData;
                                request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                                Requests.Add(request);
                                break;
                            }
                        }
                        socket.Dispose();
                    }
                    catch (Exception ex)
                    {
                        string s = ex.Message;
                        //IcmpReturn information = new IcmpReturn
                        //{
                        //    Color = "0"/
                        //};
                        request.Color = "0";
                        request.Status = "1001";
                        //string backJson = JsonConvert.SerializeObject(information);
                        //backData.Add(i.ToString(), backJson);
                        request.TimeCost = (short)stopwatch.ElapsedMilliseconds;
                        request.ErrorException = ex;
                        Requests.Add(request);
                        return false;
                    }
                }
                return true;
            }
            else
            {
                //backData.Add("ip地址不规范", "-1");
                //IcmpReturn information = new IcmpReturn
                //{
                //    Color = "0"//错误
                //};
                RequestObj request = new RequestObj
                {
                    CreateTime = DateTime.Now
                };//创建一个请求对象
                request.Color = "0";
                request.Status = "1001";
                request.TimeCost = 0;       
                return false;
            }
        }
        public IPAddress MyIPAddress { get; set; }//请求的ip地址
        public List<RequestObj> Requests = new List<RequestObj>();
    }
    class RequestObj :BasicRequest{
        //请求对象
        //一个请求对象包括  一次请求 的 创建时间、花费时间、请求结果等信息
        public String Color { get; set; }
    }
}
