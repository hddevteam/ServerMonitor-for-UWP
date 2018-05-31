using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 创建者:xb 创建时间: 2018/04
/// </summary>
namespace ServerMonitor.Models
{
    /// <summary>
    /// 创建者:xb 创建时间: 2018/04
    /// </summary>
    [Table("Site_Contact")]
    public class SiteContactModel
    {
        private int id;
        private int siteId;
        private int contactId;
        private DateTime create_time;
        private string others;

        [PrimaryKey, AutoIncrement,Column("id")]
        public int Id { get => id; set => id = value; }
        [NotNull,Column("site_id")]
        public int SiteId { get => siteId; set => siteId = value; }
        [NotNull, Column("contact_id")]
        public int ContactId { get => contactId; set => contactId = value; }
        [Default(true), Column("create_time")]
        public DateTime CreatTime { get => create_time; set => create_time = value; }
        [DefaultValue(value: "", type: typeof(string)), Column("others")]
        public string Others { get => others; set => others = value; }
    }
}
