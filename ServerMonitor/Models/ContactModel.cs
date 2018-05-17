using GalaSoft.MvvmLight;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Models
{
    // 联系人类
    [Table("Contact")]
    public class ContactModel : ObservableObject
    {
        private int id;
        // 姓名
        private string contact_name;
        // 邮箱
        private string contact_email;
        // 创建时间
        private DateTime create_time;
        // 修改时间
        private DateTime update_time;
        // 补充信息
        private string others;
        //电话
        private string telephone;
        // 关联站点
        private int siteId;

        [PrimaryKey, AutoIncrement]
        public int Id { get => id; set => id = value; }
        public string Contact_name
        {
            get => contact_name;
            set
            {
                contact_name = value;
                RaisePropertyChanged(() => Contact_name);
            }
        }
        public string Contact_email
        {
            get => contact_email;
            set
            {
                contact_email = value;
                RaisePropertyChanged(() => Contact_email);
            }
        }
        public DateTime Create_time
        {
            get => create_time;
            set
            {
                create_time = value;
                RaisePropertyChanged(() => Create_time);
            }
        }
        public DateTime Update_time
        {
            get => update_time;
            set
            {
                update_time = value;
                RaisePropertyChanged(() => Update_time);
            }
        }
        public string Others
        {
            get => others;
            set
            {
                others = value;
                RaisePropertyChanged(() => Others);
            }
        }
        public string Telephone
        {
            get => telephone;
            set
            {
                telephone = value;
                RaisePropertyChanged(() => Telephone);
            }
        }

        [NotNull]
        public int SiteId {
            get => siteId;
            set => siteId = value;
        }
    }
}
