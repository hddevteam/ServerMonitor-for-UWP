using ServerMonitor.Controls;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml.Data;
using ServerMonitor.Views;
namespace ServerMonitor.ViewModels
{
    class AddWebsitePageViewModel: ViewModelBase,INotifyPropertyChanged
    {
        public AddWebsitePageViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }
            ID = null;
        }
        private string _Value = "0";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }


        #region 响应事件
        /// <summary>
        /// 添加website进数据库
        /// </summary>
        public void AddWebsite()
        {
            //添加服务器 //Value是0代表是新增，否则value代表修改项的id
            if (ID == null)
            {
                Site _site = new Site
                {
                    Is_server = false,
                    Monitor_interval = 5,
                    Is_Monitor = true
                };
                if (HTTP == 0)
                {
                    _site.Protocol_type = "HTTP";
                }
                else if (HTTP == 1)
                {
                    _site.Protocol_type = "HTTPS";
                }
                _site.Site_address = Domain;
                try
                {
                    _site.Status_code = Status;
                }
                catch { }
                if (Name==null)
                {
                    _site.Site_name = Domain;
                }
                else
                {
                    _site.Site_name = Name;
                }
                DBHelper.InsertOneSite(_site);
                AddWebsitePage.DismissWindow();
            }
            else
            {
                Site _site = new Site
                {
                    Monitor_interval = 5,
                    Is_Monitor = true,
                    Is_server = false
                };
                try
                {
                    _site.Id = int.Parse(ID);
                }
                catch { }
                if (HTTP == 0)
                {
                    _site.Protocol_type = "HTTP";
                }
                else if (HTTP == 1)
                {
                    _site.Protocol_type = "HTTPS";
                }
                _site.Site_address = Domain;
                try
                {
                    _site.Status_code = Status;
                }
                catch { }
                if (Name==null)
                {
                    _site.Site_name = Domain;
                }
                else
                {
                    _site.Site_name = Name;
                }
                DBHelper.UpdateSite(_site);
                AddWebsitePage.DismissWindow();
            }
        }

        /// <summary>
        /// 页面加载完成后更新页面binding数据
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
                //Port = _mysite.Server_port.ToString();
                if (_mysite.Protocol_type.Equals("HTTP"))
                {
                    HTTP = 0;
                    Protocol = 0;
                }
                else if (_mysite.Protocol_type.Equals("HTTPS"))
                {
                    HTTP = 1;
                    Protocol = 1;
                }
                Status = _mysite.Status_code;
                PropertyChanged(this, new PropertyChangedEventArgs("Status"));//通知domain
                PropertyChanged(this, new PropertyChangedEventArgs("HTTP"));
                PropertyChanged(this, new PropertyChangedEventArgs("Domain"));//通知port改变
                PropertyChanged(this, new PropertyChangedEventArgs("Name"));//通知Name改变
                PropertyChanged(this, new PropertyChangedEventArgs("Protocol"));//通知port改变
            }
        }

        /// <summary>
        /// 添加status code
        /// </summary>
        public void AddCode()
        {

            if (_typecode != null)
            {
                if (_status != null)
                {
                    if (!_status.Contains(_typecode))
                    {
                        _status += "," + _typecode;
                    }
                }
                else
                {
                    _status += _typecode;
                }
            } 
            PropertyChanged(this, new PropertyChangedEventArgs("Status"));
        }

        #endregion

        #region 绑定数据

        private static string _id;

        public static string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _http;

        public int HTTP
        {
            //绑定http
            get { return _http; }
            set { _http = value; RaisePropertyChanged(() => HTTP); }
        }

        private int _protocol;

        public int Protocol
        {
            get { return _protocol; }
            set { _protocol = value; }
        }


        private string _domain;

        public string Domain
        {
            get { return _domain; }
            set { _domain = value; RaisePropertyChanged(() => Domain); }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(() => Name); }
        }

        private string _status;

        public string Status
        {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(() => Status); }
        }

        private string _typecode;

        public string TypeCode
        {
            get { return _typecode; }
            set { _typecode = value; RaisePropertyChanged(() => TypeCode); }
        }

        #endregion
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }

    /// <summary>
    /// 转换器用于转换http https 
    /// </summary>
    public class ComboBoxBoolToIntConvert : IValueConverter
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
