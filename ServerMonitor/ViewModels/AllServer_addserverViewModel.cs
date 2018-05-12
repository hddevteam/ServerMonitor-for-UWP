using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Template10.Common;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using ServerMonitor.Views;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using ServerMonitor.Controls;
using ServerMonitor.Models;

namespace ServerMonitor.ViewModels
{
    public class AllServer_addserverViewModel : ViewModelBase, INotifyPropertyChanged
    {

        public AllServer_addserverViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }
        }
        private string _Value = "0";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            //var dialog = new MessageDialog(parameter+"", "消息提示");
            //await dialog.ShowAsync();
            //AllServer_addserverViewModel.check = parameter + "";
            Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();
            //得到id值
            if (!Value.Equals("0"))
            {
               SiteModel _mysite =  DBHelper.GetSiteById(int.Parse(Value));//如果是修改站点或者服务器， 这里查询到服务器或站点的信息
                if (_mysite.Is_server == true)
                {
                    //判断是服务器还是站点    true代表是服务器
                    //禁用切换按钮
                    _togglebtn = false;
                    //修改绑定值为数据库值
                    _domain = _mysite.Site_address;
                    _servername = _mysite.Site_name;
                    _port = _mysite.Server_port.ToString();
                    if (_mysite.Protocol_type.Equals("Socket"))
                    {
                        _agreementIndex = 0;
                    }
                    else if (_mysite.Protocol_type.Equals("SSH"))
                    {
                        _agreementIndex = 1; 
                    }else if (_mysite.Protocol_type.Equals("FTP"))
                    {
                        _agreementIndex = 2;
                    } else if (_mysite.Protocol_type.Equals("DNS"))
                    {
                        _agreementIndex = 3;
                    }
                }
                else
                {
                    //是修改网站
                    //禁用切换按钮
                    _togglebtn = false;
                    _status = Visibility.Collapsed;
                    _status2 = Visibility.Visible;
                    //修改绑定值为数据库值
                    _domain = _mysite.Site_address;
                    _servername = _mysite.Site_name;
                    _port = _mysite.Server_port.ToString();
                    if (_mysite.Protocol_type.Equals("HTTP"))
                    {
                        _agreementIndex = 0;
                    }
                    else if (_mysite.Protocol_type.Equals("HTTPS"))
                    {
                        _agreementIndex = 1;
                    }
                }
            }
            else
            {
                _togglebtn = true;
            }       
            PropertyChanged(this, new PropertyChangedEventArgs("Domain"));//通知domain
            PropertyChanged(this, new PropertyChangedEventArgs("AgreementIndex"));
            PropertyChanged(this, new PropertyChangedEventArgs("Port"));//通知port改变
            PropertyChanged(this, new PropertyChangedEventArgs("ServerName"));//通知ServerName改变
            PropertyChanged(this, new PropertyChangedEventArgs("Http"));//通知Http改变
            PropertyChanged(this, new PropertyChangedEventArgs("Property"));//通知Property改变
            PropertyChanged(this, new PropertyChangedEventArgs("Header"));//通知Header改变
            PropertyChanged(this, new PropertyChangedEventArgs("State"));//通知State改变
            PropertyChanged(this, new PropertyChangedEventArgs("State2"));//通知state2改变
            PropertyChanged(this, new PropertyChangedEventArgs("ToggleBtn"));//通知ToggleBtn改变
            await Task.CompletedTask;
        }
        private Visibility _status = Visibility.Visible;//定义server grid 可见属性
        private Visibility _status2 = Visibility.Collapsed;//定义Website grid 可见属性



        public void Switch()
        {
            //绑定toggle方法，用于切换grid的可见属性
            if (_status == Visibility.Collapsed)
            {
                _status = Visibility.Visible;//改变当前属性
                _status2 = Visibility.Collapsed;//改变可见属性
                PropertyChanged(this, new PropertyChangedEventArgs("State"));//通知grid 1
                PropertyChanged(this, new PropertyChangedEventArgs("State2"));//通知grid 2
            }
            else
            {
                _status = Visibility.Collapsed;//改变可见属性
                _status2 = Visibility.Visible;//改变可见属性
                PropertyChanged(this, new PropertyChangedEventArgs("State"));//通知grid 1 
                PropertyChanged(this, new PropertyChangedEventArgs("State2"));//通知grid 2
            }

        }
        public Visibility State
        {
            get { return _status; }//返回绑定的显示状态
            set { _status = value; }
        }
        public Visibility State2
        {
            get { return _status2; }//返回绑定的显示状态
            set { _status2 = value; }
        }

        private bool _togglebtn;
        public bool ToggleBtn
        {
            get
            {
                return _togglebtn;
            }
            set
            {
                _togglebtn = value;
            }
        }

        public ObservableCollection<String> _property = new ObservableCollection<string>() { "404"};//绑定的是websit页面的status code combox 列表
        
        public ObservableCollection<String> Property
        {
            get
            {
                if (DBHelper.GetSiteById(int.Parse(Value)).Request_succeed_code != null)
                {
                    string obj = DBHelper.GetSiteById(int.Parse(Value)).Request_succeed_code;
                    string[] str = obj.Split(new char[] { ','});
                    foreach (var item in str)
                    {
                        _property.Add(item);
                    }
                    return _property;
                }
                else
                {
                    return _property;
                }
            }
            set
            {
                _property = value;
            }
        }
        
        public String SimpleStringProperty { get; set; }
        public void addCodeBtn_Click(object sender, RoutedEventArgs e)
        {
            //添加监听端口号
            //添加监听端口号
            ComboBox combo = new ComboBox();
            string codeText = "";
            codeText = CodeText;
            _property.Add(codeText);
            PropertyChanged(this, new PropertyChangedEventArgs("Property"));//通知property
            //this.StatusCodeCombox.Items.Add(codeText);
            //this.addCodeStatusText.Text = "";
            onAddCode();
            CodeText = "";
            PropertyChanged(this, new PropertyChangedEventArgs("CodeText"));//通知codetext

        }
        private async void onAddCode()
        {
            MessageDialog dialog = new MessageDialog("添加完成");
            //MessageDialog dialog = new MessageDialog(_property+"");
            dialog.Commands.Add(new UICommand("确定", cmd => { }, commandId: 0));
            await dialog.ShowAsync();
        }

        private string _codetext="";
        public string CodeText { get { return _codetext; } set { _codetext = value; } }



        private string _domain;
        public string Domain
        {//绑定server domain

            get
            {
                return _domain;            
            }
            set { _domain = value; }
        }
        
        public void Remind()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("AgreementIndex"));
        }
        private int _agreementIndex;//标志这个服务器的协议是 DEFAULT SSH FTP OR
        public int AgreementIndex//这个方法绑定了 添加server页面的 协议类型 index
        {
            get
            {
                //if (_agreementIndex == 0)
                //{
                //    if (DBHelper.GetSiteById(int.Parse(Value)).Protocol_type != null)
                //    {
                //        if (DBHelper.GetSiteById(int.Parse(Value)).Protocol_type.Equals("Socket"))
                //        {
                //            _agreementIndex = 0;
                //            return _agreementIndex;
                //        }
                //        else if (DBHelper.GetSiteById(int.Parse(Value)).Protocol_type.Equals("SSH"))
                //        {
                //            _agreementIndex = 1;
                //            return _agreementIndex;
                //        }
                //        else if (DBHelper.GetSiteById(int.Parse(Value)).Protocol_type.Equals("FTP"))
                //        {
                //            _agreementIndex = 2;
                //            return _agreementIndex;
                //        }
                //        else if (DBHelper.GetSiteById(int.Parse(Value)).Protocol_type.Equals("DNS"))
                //        {
                //            _agreementIndex = 3;
                //            return _agreementIndex;
                //        }
                //        else
                //        {
                //            _agreementIndex = 0;
                //            return _agreementIndex;
                //        }
                //    }
                //    else
                //    {
                //        return _agreementIndex;
                //    }
                //}
                //else
                //{
                //    return _agreementIndex;
                //}
                return _agreementIndex;
                
            }
            set
            {
                _agreementIndex = value;
            }
        }
        private string _port;//绑定添加server页面 服务器port号
        public string Port
        {
            get
            {
                //if (_port == null)
                //{
                //    if (DBHelper.GetSiteById(int.Parse(Value)).Server_port != 0)
                //    {
                //        _port = DBHelper.GetSiteById(int.Parse(Value)).Server_port.ToString();
                //        return _port;//
                //    }
                //    else
                //    {
                //        return _port;
                //    }
                //}
                //else
                //{
                //    return _port;
                //}
                return _port;
           
            }
            set
            {
                _port = value;
            }
        }
        private string _servername;
        public string ServerName
        {
            //绑定服务器名字
            get
            {

                //if (_servername==null)
                //{
                //    if (DBHelper.GetSiteById(int.Parse(Value)).Site_name != null)
                //    {
                //        _servername = DBHelper.GetSiteById(int.Parse(Value)).Site_name;
                //        return _servername;
                //    }
                //    else
                //    {
                //        return _servername;
                //    }
                //}
                //else
                //{
                //    return _servername;
                //}
                return _servername;
            }
            set { _servername = value;
            }
        }

        /// <summary>
        /// WEBSITE 页面绑定值
        /// </summary>
        private int _http;
        public int Http
        {
            get
            {
                return _http;
            }
            set { _http = value; }
        }

        public void AddServerBtn()
        {
            //添加服务器 //Value是0代表是新增，否则value代表修改项的id
            if (Value.Equals("0"))
            {
                SiteModel _site = new SiteModel();
                _site.Is_server = true;
                _site.Monitor_interval = 5;
                _site.Is_Monitor = true;
                if (_agreementIndex == 0)
                {
                    _site.Protocol_type = "socket";
                }
                else if (_agreementIndex == 1)
                {
                    _site.Protocol_type = "SSH";
                }
                else if (_agreementIndex == 2)
                {
                    _site.Protocol_type = "FTP";
                }
                else if (_agreementIndex == 3)
                {
                    _site.Protocol_type = "DNS";
                }
                _site.Site_address = _domain;
                try
                {
                    _site.Server_port = int.Parse(_port);
                }
                catch { }
                if (ServerName.Equals(""))
                {
                    _site.Site_name = _domain;
                }
                else
                {
                    _site.Site_name = _servername;
                }
                DBHelper.InsertOneSite(_site);
                GotoAllserver();//添加后返回
            }
            else
            {
                SiteModel _site = new SiteModel();
                try
                {
                    _site.Id = int.Parse(Value);
                }
                catch { }
                _site.Monitor_interval = 5;
                _site.Is_Monitor = true;
                _site.Is_server = true;
                if (_agreementIndex == 0)
                {
                    _site.Protocol_type = "socket";
                }
                else if (_agreementIndex == 1)
                {
                    _site.Protocol_type = "SSH";
                }
                else if (_agreementIndex == 2)
                {
                    _site.Protocol_type = "FTP";
                }
                else if (_agreementIndex == 3)
                {
                    _site.Protocol_type = "DNS";
                }
                _site.Site_address = _domain;
                try
                {
                    _site.Server_port = int.Parse(Port);
                }
                catch { }
                if (ServerName.Equals(""))
                {
                    _site.Site_name = _domain;
                }
                else
                {
                    _site.Site_name = _servername;
                }
                DBHelper.UpdateSite(_site);
                GotoAllserver();//修改后返回
            }
           
        }
        public void AddWebsiteBtn()
        {
            //添加website  // Value 为0是新增，Value 不为0 则代表修改的id
            if (Value.Equals("0"))
            {
                SiteModel _site = new SiteModel();
                _site.Is_server = false;
                _site.Monitor_interval = 5;
                _site.Is_Monitor = true;
                if (Http == 1)
                {
                    _site.Protocol_type = "HTTP";
                }
                else if (Http == 2)
                {
                    _site.Protocol_type = "HTTPS";
                }
                else
                {
                    _site.Protocol_type = "HTTP";
                }
                _site.Site_address = _domain;
                //_site.Status_code = Property.ToList()[1];
                foreach (var s in Property)
                {
                    _site.Request_succeed_code += s + ",";
                }
                if (ServerName.Equals(""))
                {
                    _site.Site_name = _domain;
                }
                else
                {
                    _site.Site_name = _servername;
                }
                DBHelper.InsertOneSite(_site);
                GotoAllserver();//添加后返回
            }
            else
            {
                SiteModel _site = new SiteModel();
                _site.Is_server = false;
                _site.Monitor_interval = 5;
                _site.Is_Monitor = true;
                try
                {
                    _site.Id = int.Parse(Value);
                }
                catch { }
                if (Http == 1)
                {
                    _site.Protocol_type = "HTTP";
                }
                else if (Http == 2)
                {
                    _site.Protocol_type = "HTTPS";
                }
                else
                {
                    _site.Protocol_type = "HTTP";
                }
                _site.Site_address = _domain;
                //_site.Status_code = Property.ToList()[1];
                foreach (var s in Property)
                {
                    _site.Request_succeed_code += s + ",";
                }
                if (ServerName.Equals(""))
                {
                    _site.Site_name = _domain;
                }
                else
                {
                    _site.Site_name = _servername;
                }
                DBHelper.UpdateSite(_site);
                GotoAllserver();//修改后返回
            }
            

        }
        public void GotoAllserver() =>
            NavigationService.Navigate(typeof(Views.AllServer), 0);
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

    }
}
