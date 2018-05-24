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
        /// <summary>
        /// 姓名
        /// </summary>
        private string contact_name;
        /// <summary>
        /// 邮箱
        /// </summary>
        private string contact_email;
        /// <summary>
        /// 创建时间
        /// </summary>
        private DateTime create_time;
        /// <summary>
        /// 修改时间
        /// </summary>
        private DateTime update_time;
        /// <summary>
        /// 补充信息
        /// </summary>
        private string others;
        /// <summary>
        /// 电话
        /// </summary>
        private string telephone;

        [PrimaryKey, AutoIncrement,Column("id")]
        public int Id { get => id; set => id = value; }
        [Column("contact_name")]
        public string Contact_name
        {
            get => contact_name;
            set
            {
                contact_name = value;
                RaisePropertyChanged(() => Contact_name);
            }
        }
        [Column("contact_email")]
        public string Contact_email
        {
            get => contact_email;
            set
            {
                contact_email = value;
                RaisePropertyChanged(() => Contact_email);
            }
        }
        [Column("create_time"), Default(true)]
        public DateTime Create_time
        {
            get => create_time;
            set
            {
                create_time = value;
                RaisePropertyChanged(() => Create_time);
            }
        }
        [Column("update_time"), Default(true)]
        public DateTime Update_time
        {
            get => update_time;
            set
            {
                update_time = value;
                RaisePropertyChanged(() => Update_time);
            }
        }
        [Column("others")]
        public string Others
        {
            get => others;
            set
            {
                others = value;
                RaisePropertyChanged(() => Others);
            }
        }
        [Column("telephone")]
        public string Telephone
        {
            get => telephone;
            set
            {
                telephone = value;
                RaisePropertyChanged(() => Telephone);
            }
        }
    }
}
