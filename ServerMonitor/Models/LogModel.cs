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
    [Table("Log")]
    public class LogModel : ObservableObject
    {
        int id;
        int site_id;
        string status_code;
        double timeCost;
        DateTime create_time;
        bool is_error;
        string log_record;

        [PrimaryKey,AutoIncrement,Column("id")]
        public int Id
        {
            get => id;
            set => id = value;
        }
        [Column("site_id")]
        public int Site_id { get => site_id;
            set
            {
                site_id = value;
                RaisePropertyChanged(() => Site_id);
            }
        }
        [Column("status_code")]
        public string Status_code { get => status_code;
            set
            {
                status_code = value;
                RaisePropertyChanged(() => Status_code);
            }
        }
        [Column("timecost")]
        public double TimeCost{
            get => timeCost;
            set
            {
                timeCost = value;
                RaisePropertyChanged(() => TimeCost);
            }
        }
        [Column("create_time")]
        public DateTime Create_Time {
            get => create_time;
            set
            {
                if (!value.Equals(null))
                {
                    create_time = value;
                    RaisePropertyChanged(() => Create_Time);
                }
                else
                {
                    create_time = DateTime.Now;
                    RaisePropertyChanged(() => Create_Time);
                }
            }
        }
        [Column("is_error")]
        public bool Is_error { get => is_error;
            set
            {
                is_error = value;
                RaisePropertyChanged(() => Is_error);
            }
        }
        [Column("log_record")]
        public string Log_Record { get => log_record;
            set
            {
                log_record = value;
                RaisePropertyChanged(() => Log_Record);
            }
        }

        public override string ToString()
        {
            return string.Format("站点:{0}\t创建时间:{3}\t时间:{1}\t请求状态码:{2}",Site_id,timeCost,status_code,create_time.ToLocalTime());
        }
    }
}
