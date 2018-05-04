using ServerMonitor.Controls;
using ServerMonitor.Models;
using ServerMonitor.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace ServerMonitor.ViewModels
{
    class AddServerPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public AddServerPageViewModel()
        {
            ID = null;     //初始化清空id   
        }

        #region 绑定数据
        private string _id;   //站点id
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _domain; //域名绑定
        public string Domain
        {
            get { return _domain; }
            set { _domain = value; }
        }

        private string _port;
        public string Port //端口绑定
        {
            get { return _port; }
            set { _port = value; }
        }

        private string _name;
        public string Name //服务器名称绑定
        {
            get { return _name; }
            set { _name = value; }
        }

        //绑定协议
        private int _protocol;//1socket 2ssh 3ftp 4dns
        public int Protocol { get { return _protocol; } set { _protocol = value; } }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        #endregion

        #region 响应事件
        /// <summary>
        /// 在页面加载完成后，更新页面bind数据
        /// </summary>
        public void Updatedata()
        {
            if (ID != null)
            {
                //id不为空,则从数据库查询相关数据进行binding
                Site _mysite = DBHelper.GetSiteById(int.Parse(ID));//如果是修改站点或者服务器， 这里查询到服务器或站点的信息
                                                                   //修改绑定值为数据库值
                Domain = _mysite.Site_address;
                Name = _mysite.Site_name;
                Port = _mysite.Server_port.ToString();
                if (_mysite.Protocol_type.Equals("Socket"))
                {
                    Protocol = 1;
                }
                else if (_mysite.Protocol_type.Equals("SSH"))
                {
                    Protocol = 2;
                }
                else if (_mysite.Protocol_type.Equals("FTP"))
                {
                    Protocol = 3;
                }
                else if (_mysite.Protocol_type.Equals("DNS"))
                {
                    Protocol = 4;
                }
                else if (_mysite.Protocol_type.Equals("ICMP"))
                {
                    Protocol = 5;
                }
                PropertyChanged(this, new PropertyChangedEventArgs("Domain"));//通知domain
                PropertyChanged(this, new PropertyChangedEventArgs("Protocol"));
                PropertyChanged(this, new PropertyChangedEventArgs("Port"));//通知Port改变
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));//通知Name改变
            }
        }
        /// <summary>
        /// 添加服务器方法
        /// </summary>
        public void AddServer()
        {
            Site _site = new Site
            {
                Is_server = true,
                Monitor_interval = 5,
                Is_Monitor = true
            };
            if (Protocol == 1)
            {
                _site.Protocol_type = "Socket";
            }
            else if (Protocol == 2)
            {
                _site.Protocol_type = "SSH";
            }
            else if (Protocol == 3)
            {
                _site.Protocol_type = "FTP";
            }
            else if (Protocol == 4)
            {
                _site.Protocol_type = "DNS";
            }
            else if (Protocol == 5)
            {
                _site.Protocol_type = "ICMP";
            }
            _site.Site_address = Domain;
            try
            {
                _site.Server_port = int.Parse(Port);
            }
            catch { }
            if (Name == null)
            {
                _site.Site_name = Domain;
            }
            else
            {
                _site.Site_name = Name;
            }
            //添加服务器 //Value是0代表是新增，否则value代表修改项的id
            if (ID == null)
            {
                DBHelper.InsertOneSite(_site);
                AddServerPage.DismissWindow();
            }
            else
            {
                try
                {
                    _site.Id = int.Parse(ID);
                }
                catch { }
                DBHelper.UpdateSite(_site);
                AddServerPage.DismissWindow();
            }
        }
        #endregion
    }
    /// <summary>
    /// 转换器，用于转换协议类型
    /// </summary>
    public class RadioBoolToIntConverter : IValueConverter
    {
        //协议转换器
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int integer = (int)value;
            if (integer == int.Parse(parameter.ToString()))
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return parameter;
        }
    }
}
