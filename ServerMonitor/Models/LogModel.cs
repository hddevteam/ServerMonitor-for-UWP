using GalaSoft.MvvmLight;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Models
{
    // 对应数据库里的Log表
    public class LogModel : ObservableObject
    {
        int id;
        int site_id;
        string status_code;
        double request_time;
        DateTime create_time;
        bool is_error;
        string log_record;

        [PrimaryKey,AutoIncrement]
        public int Id
        {
            get => id;
            set => id = value;
        }
        public int Site_id { get => site_id;
            set
            {
                site_id = value;
                RaisePropertyChanged(() => Site_id);
            }
        }
        public string Status_code { get => status_code;
            set
            {
                status_code = value;
                RaisePropertyChanged(() => Status_code);
            }
        }
        public double Request_time {
            get => request_time;
            set
            {
                request_time = value;
                RaisePropertyChanged(() => Request_time);
            }
        }
        public DateTime Create_time {
            get => create_time;
            set
            {
                if (!value.Equals(null))
                {
                    create_time = value;
                    RaisePropertyChanged(() => Create_time);
                }
                else
                {
                    create_time = DateTime.Now;
                    RaisePropertyChanged(() => Create_time);
                }
            }
        }
        public bool Is_error { get => is_error;
            set
            {
                is_error = value;
                RaisePropertyChanged(() => Is_error);
            }
        }
        public string Log_record { get => log_record;
            set
            {
                log_record = value;
                RaisePropertyChanged(() => Log_record);
            }
        }

        public override string ToString()
        {
            return string.Format("站点:{0}\t时间:{1}\t请求状态码:{2}",Site_id,request_time,status_code);
        }
    }
}
