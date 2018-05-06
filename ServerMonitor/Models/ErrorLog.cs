using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Models
{
    // 错误日志对象
    public class ErrorLog
    {        
        int id;
        // 异常类型
        string exceptionType = "";
        // 异常内容
        string exceptionContent = "";
        // 异常发生时间
        DateTime createTime;
        // 其他信息
        string others;

        //  属性对应的字段👇
        [PrimaryKey, AutoIncrement]
        public int Id {
            get => id;            
        }
        public string ExceptionType {
            get => exceptionType;
            set => exceptionType = value;
        }
        public string ExceptionContent {
            get => exceptionContent;
            set => exceptionContent = value;
        }
        [Default(true,typeof(DateTime))]
        public DateTime CreateTime {
            get => createTime;
            set => createTime = value;
        }
        public string Others {
            get => others;
            set => others = value;
        }

        public override string ToString()
        {
            return string.Format("时间：{0}\t错误类型：{1}\t错误内容:{2}", CreateTime.ToString(),ExceptionType,ExceptionContent);
        }

    }
}
