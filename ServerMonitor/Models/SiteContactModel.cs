using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Models
{
    [Table("SiteContact")]
    public class SiteContactModel
    {
        private int id;
        private int siteId;
        private int contactId;
        private DateTime bindTime;
        private string others;

        [PrimaryKey, AutoIncrement,Column("id")]
        public int Id { get => id; set => id = value; }
        [NotNull,Column("site_id")]
        public int SiteId { get => siteId; set => siteId = value; }
        [NotNull, Column("contact_id")]
        public int ContactId { get => contactId; set => contactId = value; }
        [Default(true), Column("create_time")]
        public DateTime BindTime { get => bindTime; set => bindTime = value; }
        [DefaultValue(value: "", type: typeof(string)), Column("others")]
        public string Others { get => others; set => others = value; }
    }
}
