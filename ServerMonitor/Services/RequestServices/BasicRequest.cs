using System;

namespace ServerMonitor.Services.RequestServices
{
    public abstract class BasicRequest
    {
        public readonly double ErrorQuality = 1.5;
        /// <summary>
        /// 请求创建的时间
        /// </summary>
        private DateTime createTime;
        /// <summary>
        /// 请求花费的时间
        /// </summary>
        private short timeCost;
        /// <summary>
        /// 定义的请求超时时长
        /// </summary>
        private short overTime = 5000;
        /// <summary>
        /// 请求返回的结果
        /// </summary>
        private string status;
        /// <summary>
        /// 请求的额外信息
        /// </summary>
        private string others;
        /// <summary>
        /// 请求出现的异常
        /// </summary>
        private Exception errorException;       

        public DateTime CreateTime { get => createTime; set => createTime = value; }
        public short TimeCost { get => timeCost; set => timeCost = value; }
        public short OverTime { get => overTime; set => overTime = value; }
        public string Status { get => status; set => status = value; }
        public string Others { get => others; set => others = value; }
        public Exception ErrorException { get => errorException; set => errorException = value; }
        
    }
}
