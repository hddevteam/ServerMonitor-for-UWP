using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMonitor.Models
{
    [Table("ContactSite")]
    public class ContactSiteModel
    {
        private int id;
        private int siteId;
        private int contactId;
        private DateTime bindTime;
        private string others;

        [PrimaryKey, AutoIncrement]
        public int Id { get => id; set => id = value; }
        [NotNull]
        public int SiteId { get => siteId; set => siteId = value; }
        [NotNull]
        public int ContactId { get => contactId; set => contactId = value; }
        [Default(true)]
        public DateTime BindTime { get => bindTime; set => bindTime = value; }
        [DefaultValue(value: "", type: typeof(string))]
        public string Others { get => others; set => others = value; }
    }
}
