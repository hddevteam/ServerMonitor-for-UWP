using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Services.RequestServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Controls
{
    class DataHelper
    {
        //用于处理ICMP 返回数据包
        /// <summary>
        /// 用于获取ICMP数据包中的颜色属性，存在通则为通
        /// </summary>
        /// <param name="dictionary">传入返回的ICMP数据包</param>
        /// <returns>返回状态颜色</returns>
        public static string GetColor(Dictionary<string, string> dictionary)
        {
            if (dictionary.Count() == 1)
            {
                string color = "";
                foreach (var data in dictionary)
                {
                    JObject js = (JObject)JsonConvert.DeserializeObject(data.Value);
                    color = js["Color"].ToString();//获取颜色                   
                }
                return color;
            }
            else
            {
                string[] theColor = new string[5];
                int num = 0;
                //dictionary 中存在 5 个颜色
                foreach (var datacolor in dictionary)
                {
                    JObject js = (JObject)JsonConvert.DeserializeObject(datacolor.Value);
                    theColor[num] = js["Color"].ToString();//颜色放入数组
                    num++;
                }
                //(Red：0,错误)  (Orange：-1 超时) (Gray：2,未知)   (Blue：1成功)
                if ("2".Equals(theColor[0]) || "2".Equals(theColor[1]) || "2".Equals(theColor[2]) || "2".Equals(theColor[3]) || "2".Equals(theColor[4]))
                {
                    return "2";
                }
                else if ("1".Equals(theColor[0]) || "1".Equals(theColor[1]) || "1".Equals(theColor[2]) || "1".Equals(theColor[3]) || "1".Equals(theColor[4]))
                {
                    //(Red：0,错误)  (Orange：-1 超时) (Gray：2,未知)   (Blue：1成功)
                    //存在ping通的        
                    return "1";
                }
                else if ("0".Equals(theColor[0]) || "0".Equals(theColor[1]) || "0".Equals(theColor[2]) || "0".Equals(theColor[3]) || "0".Equals(theColor[4]))
                {
                    return "0";
                }
                else
                {
                    return "-1";
                }
            }
        }
        /// <summary>
        /// 用于获取IMCP数据包中的时间属性
        /// </summary>
        /// <param name="dictionary">传入返回的ICMP数据包</param>
        /// <returns>返回时间均值</returns>
        public static string GetTime(Dictionary<string,string> dictionary)
        {
            int  _totalTime=0;
            int _singleTime = 0;            
            //根据数据包获取颜色值
            if (dictionary.Count == 1)
            {
                foreach (var datatime in dictionary)
                {
                    JObject js = (JObject)JsonConvert.DeserializeObject(datatime.Value);
                    if (!js["Time"].ToString().Equals(""))
                    {
                        _totalTime = int.Parse(js["Time"].ToString());
                    }
                    else
                    {
                        _totalTime = 0;
                    }
                }
                _singleTime = _totalTime;
            }
            else
            {
                foreach (var datatime in dictionary)
                {
                    JObject js = (JObject)JsonConvert.DeserializeObject(datatime.Value);
                    if (!js["Time"].ToString().Equals(""))
                    {
                        _totalTime += int.Parse(js["Time"].ToString());
                    }
                    else
                    {
                        _totalTime += 0;
                    }
                }
                _singleTime = _totalTime / 5;
            }


             return _singleTime.ToString();
        }

        /// <summary>
        /// 用于获取 HTTP 请求返回数据中的颜色值
        /// </summary>
        /// <param name="backData">传入返回的json字符串</param>
        /// <returns>返回状态颜色</returns>
        public static string GetHttpColor(string backData)
        {
            JObject js = (JObject)JsonConvert.DeserializeObject(backData);
            string color = js["Color"].ToString();
            return color;
        }
        /// <summary>
        /// 用于获取HTTP 请求返回数据中的请求时间
        /// </summary>
        /// <param name="backData">传入返回的json字符串</param>
        /// <returns>返回请求时间</returns>
        public static string GetHttpTime(string backData)
        {
            JObject js = (JObject)JsonConvert.DeserializeObject(backData);
            string time = js["RequestTime"].ToString();
            return time;
        }

        /// <summary>
        /// 用户获取HTTP 请求返回数据中的状态码
        /// </summary>
        /// <param name="backData"></param>
        /// <returns></returns>
        public static string GetHttpStatus(string backData)
        {
            JObject js = (JObject)JsonConvert.DeserializeObject(backData);
            string status = js["StatusCode"].ToString();
            return status;
        }
        /// <summary>
        /// 用于对icmp请求后获取的对象属性进分析，对5条信息 整合成为一条信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static RequestObj GetProperty(ICMPRequest request)
        {
            List<RequestObj> datas = new List<RequestObj>();
            datas = request.Requests;
            RequestObj obj = new RequestObj
            {
                TimeCost = 0
            };
            int count = datas.Count();
            for (int i = 0; i < count; i++)
            {
                if (obj.Color != null)
                {
                    if (obj.Color.Equals("2"))
                    {
                        obj.Color = "2";
                        obj.Status = "1002";
                    }
                    else if (obj.Color.Equals("0"))
                    {
                        obj.Color = "0";
                        obj.Status = "1001";
                    }
                    else if (obj.Color.Equals("1"))
                    {
                        obj.Color = "1";
                        obj.Status = "1000";
                    }
                    else
                    {
                        obj.Color = "-1";
                        obj.Status = "1002";
                    } 
                }
                else
                {
                    obj.Color = datas[i].Color; 
                    switch (datas[i].Color)
                    {
                        case "0":
                            obj.Status = "1001";
                            break;
                        case "1":
                            obj.Status = "1000";
                            break;
                        case "-1":
                            obj.Status = "1002";
                            break;
                        case "2":
                            obj.Status = "1002";
                            break;
                    }

                }
                obj.TimeCost = short.Parse((short.Parse(((obj.TimeCost*i) + datas[i].TimeCost).ToString())/short.Parse((i+1).ToString())).ToString());
            }
            return obj;
            
        }
    }
}
