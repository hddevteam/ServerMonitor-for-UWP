using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Controls;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Navigation;

namespace ServerMonitor.ViewModels
{
    class AddServerPageViewModel : ViewModelBase, INotifyPropertyChanged
    {
        public AddServerPageViewModel() //先-> ComboBox_SelectionChanged-> OnNavigatedToAsync-> OnLoaded
        {
            ChangeDisplay(0);
            //因为其中的Contacts.Add(list[i]);另开了线程，所以如果晚执行比如在OnLoaded中，会导致Contacts中短时间内没有数据
            //而在这个短时间内，触发BindContact_Click会导致(ListViewItem)contactList.ContainerFromIndex(i)为null
            GetListContact();
        }
        #region 全局变量
        private ListView contactList;  //联系人列表
        private Grid rightFrame1;    //右部侧边栏
        Dictionary<int, bool> vs = new Dictionary<int, bool>(); //记录绑定联系人的结果
        Dictionary<int, bool> tempVs = new Dictionary<int, bool>(); //记录绑定联系人过程中的选择变化
        int page = 1;  //1MainPage, 2 AllServerPage
        int siteId = -1;  //-1没有id是新建site  ，不为-1代表Edit那个站点

        private string _Value = "Default";  //保存传过来的信息  为 "page,siteId"
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }
        #endregion

        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();
            await Task.CompletedTask;
            await Task.Run(()=> {    // OnNavigatedToAsync为异步方法，与OnLoaded谁先谁后不一定
                string[] arr = Value.Split(',');
                try
                {
                    page = int.Parse(arr[0]);
                    siteId = int.Parse(arr[1]);
                }
                catch (Exception)
                {        //解析错误，page=1，任务完成跳MainPage，siteId = -1无id新建站点
                    page = 1;
                    siteId = -1;
                }
                if (siteId != -1)
                {
                    GetEditSite();  //需要id 放这里
                }
            });
        }

        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            if (suspending)
            {
                suspensionState[nameof(Value)] = Value;
            }
            await Task.CompletedTask;
        }

        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
        #endregion 系统函数

        #region 绑定数据

        #region UI控件的显示与其他
        private Boolean livePort;  //Port是否可人为输入 true 可输入
        public Boolean LivePort
        {
            get => livePort;
            set
            {
                livePort = value;
                RaisePropertyChanged(() => LivePort);
            }
        }
        private Boolean diePort;  // = ! livePort
        public Boolean DiePort
        {
            get => diePort;
            set
            {
                diePort = value;
                RaisePropertyChanged(() => DiePort);
            }
        }

        private Boolean needUser;  //是否需要用户名和密码
        public Boolean NeedUser
        {
            get => needUser;
            set
            {
                needUser = value;
                RaisePropertyChanged(() => NeedUser);
            }
        }

        private Boolean needRecord;   //是否需要Record
        public Boolean NeedRecord
        {
            get => needRecord;
            set
            {
                needRecord = value;
                RaisePropertyChanged(() => NeedRecord);
            }
        }

        private Boolean isEnabled;   //save按钮是否可用
        public Boolean IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }
        #endregion

        private ObservableCollection<Contact> contacts = new ObservableCollection<Contact>(); //所有联系人，在本界面只添加一次数据
        public ObservableCollection<Contact> Contacts { get => contacts; set => contacts = value; }

        private ObservableCollection<Contact> selectedContacts = new ObservableCollection<Contact>();  //选中的绑定联系人
        public ObservableCollection<Contact> SelectedContacts { get => selectedContacts; set => selectedContacts = value; }

        private int protocolType = 0;
        public int ProtocolType
        {
            get => protocolType;
            set
            {
                protocolType = value;
                RaisePropertyChanged(() => ProtocolType);
            }
        }

        private string siteAddress;
        public string SiteAddress
        {
            get => siteAddress;
            set
            {
                siteAddress = value;
                RaisePropertyChanged(() => SiteAddress);
            }
        }

        private string siteName;
        public string SiteName
        {
            get => siteName;
            set
            {
                siteName = value;
                RaisePropertyChanged(() => SiteName);
            }
        }

        private int port;
        public int Port
        {
            get => port;
            set
            {
                port = value;
                RaisePropertyChanged(() => Port);
            }
        }
        //用户名和密码
        private string username;
        public string Username
        {
            get => username;
            set
            {
                username = value;
                RaisePropertyChanged(() => Username);
            }
        }
        private string password;
        public string Password
        {
            get => password;
            set
            {
                password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        //RECORD
        private int recordType;
        public int RecordType
        {
            get => recordType;
            set
            {
                recordType = value;
                RaisePropertyChanged(() => RecordType);
            }
        }
        private string lookup;
        public string Lookup
        {
            get => lookup;
            set
            {
                lookup = value;
                RaisePropertyChanged(() => Lookup);
            }
        }
        private string expectedResults;
        public string ExpectedResults
        {
            get => expectedResults;
            set
            {
                expectedResults = value;
                RaisePropertyChanged(() => ExpectedResults);
            }
        }

        //PRIORITY
        private int priority;
        public int Priority
        {
            get => priority;
            set
            {
                priority = value;
                RaisePropertyChanged(() => Priority);
            }
        }
        #endregion

        #region 响应事件
        /// <summary>
        /// 协议下拉框的改变触发 比onLoad 和OnNavigatedToAsync先运行
        /// </summary>
        public void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeDisplay((sender as ComboBox).SelectedIndex);
        }
        /// <summary>
        /// BindContact按钮点击事件 呼出侧边栏
        /// </summary>
        public void BindContact_Click(object sender, RoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Visible;
            for (int i = 0; i < contactList.Items.Count; i++)
            {
                ListViewItem vss = (ListViewItem)contactList.ContainerFromIndex(i);
                if (vss != null) //有时数据没有初始化完全，所以可能为空
                {
                    //在contactList的item里添加新控件时在这一步报错时，关闭vs，再打开看看
                    vss.IsSelected = vs[Contacts[i].Id];//根据vs设置contactList选中效果 
                    tempVs[Contacts[i].Id] = vs[Contacts[i].Id];
                }
            }
        }

        /// <summary>
        /// 侧边栏Cancel按钮点击事件
        /// </summary>
        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 侧边栏Ok按钮点击事件
        /// </summary>
        public void Ok_Click(object sender, RoutedEventArgs e)
        {
            var list = contactList.SelectedItems;
            rightFrame1.Visibility = Visibility.Collapsed;
            foreach (var item in tempVs)  //将结果交付给vs
            {
                vs[item.Key] = item.Value;
            }
            SelectedContacts.Clear();
            for (int i = 0; i < Contacts.Count; i++)  //根据vs刷新选中联系人
            {
                if (vs[Contacts[i].Id])
                {
                    SelectedContacts.Add(Contacts[i]);
                }
            }
        }
        
        /// <summary>
        /// 联系人列表item点击事件
        /// </summary>
        public void Contactlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (Contact)e.ClickedItem;
            tempVs[item.Id] = !tempVs[item.Id];
        }

        /// <summary>
        /// 判断Domain是否合法
        /// </summary>
        public void Domain_TextChanged(object sender, TextChangedEventArgs e)
        {
            string domain = (sender as TextBox).Text.ToString();
            IsEnabled = CheckDomain(domain);
        }

        /// <summary>
        /// 上传提交，保存到数据库
        /// </summary>
        public void Save()
        {
            Site site;
            if (siteId == -1)  //新建Site
            {
                site = new Site()
                {
                    Is_server = true,
                    Monitor_interval = 5,
                    Is_Monitor = true,
                    Create_time = DateTime.Now,
                    Last_request_result = 2,
                    Status_code = "1000/0",
                    Request_succeed_code = "1000",
                };
            }
            else    //Edit site
            {
                site = DBHelper.GetSiteById(siteId);
            }
            site.Update_time = DateTime.Now;

            //将界面数据保存下来
            site.Protocol_type = GetProtocolType(ProtocolType);
            site.Site_address = SiteAddress;
            site.Server_port = Port;
            if (SiteName == null|| SiteName.Equals(""))
            {
                site.Site_name = SiteAddress;
            }
            else
            {
                site.Site_name = SiteName;
            }
            if (site.Protocol_type.Equals("SSH")|| site.Protocol_type.Equals("FTP"))
            {
                site.ProtocolIdentification = GetJson(Username, Password);
            }
            else if(site.Protocol_type.Equals("DNS"))
            {
                site.ProtocolIdentification = GetJson(RecordType, Lookup, ExpectedResults);
            }

            //数据库操作
            if (siteId == -1)
            {
                if (DBHelper.InsertOneSite(site) == 1) 
                {
                    Jump(); //返回原界面
                }
            }
            else
            {
                if (DBHelper.UpdateSite(site) == 1)
                {
                    Jump();
                }
            }
        }

        /// <summary>
        /// 取消修改/添加 返回原界面
        /// </summary>
        public void CancelBack()
        {
            Jump();
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// OnNavigatedTo后调用 UI控件对象传递
        /// </summary>
        public void OnLoaded(ListView contactList,Grid grid)
        {
            rightFrame1 = grid;   //侧边栏
            this.contactList = contactList;   //侧边栏联系人列表
            rightFrame1.Visibility = Visibility.Collapsed;  //关闭侧边栏
        }

        /// <summary>
        /// 获取联系人列表 初始化变量 只调用一次
        /// </summary>
        private void GetListContact()  //不可测
        {
            List<Contact> list = DBHelper.GetAllContact();
            //Contacts.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                vs.Add(list[i].Id, false);
                tempVs.Add(list[i].Id, false);
                Contacts.Add(list[i]);
            }
        }

        /// <summary>
        /// UI控件的显示
        /// </summary>
        private void ChangeDisplay(int i)
        {
            switch (i)
            {
                case 0:   //ICMP
                    LivePort = true;
                    DiePort = false;
                    NeedUser = false;
                    NeedRecord = false;
                    break;
                case 1:     //SOCKET
                    LivePort = true;
                    DiePort = false;
                    NeedUser = false;
                    NeedRecord = false;
                    break;
                case 2:    //SSH
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    Port = 22;
                    break;
                case 3:   //FTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    Port = 21;
                    break;
                case 4:    //DNS
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = true;
                    Port = 53;
                    break;
                case 5:    //SMTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = false;
                    Port = 25;
                    break;
                default:
                    break;
            }
        }

        //在siteId!=-1时调用， 即Edit site 时调用
        /// <summary>
        /// Edit site 时调用 将数据库数据分配到绑定字段里
        /// </summary>
        private void GetEditSite()
        {
            Site site = DBHelper.GetSiteById(siteId);
            ProtocolType = GetProtocolType(site.Protocol_type);
            SiteAddress = site.Site_address;
            SiteName = site.Site_name;
            Port = site.Server_port;
            
            if (ProtocolType == 2 || ProtocolType == 3)
            {
                JObject js = (JObject)JsonConvert.DeserializeObject(site.ProtocolIdentification);
                Username = js["useaname"].ToString();//用户名
                Password = js["password"].ToString();//用户名
            }
            else if(ProtocolType == 4)
            {
                JObject js = (JObject)JsonConvert.DeserializeObject(site.ProtocolIdentification);
                try
                {
                    RecordType = int.Parse(js["recordType"].ToString());
                }
                catch (Exception)
                {
                    RecordType = 0;//出错 默认选第一个
                }
                Lookup = js["lookup"].ToString();
                ExpectedResults = js["expectedResults"].ToString();
            }
        }

        /// <summary>
        /// 检验地址是否合法
        /// </summary>
        /// <param name="domain">地址字符串</param>
        /// <returns>是否合法</returns>
        private bool CheckDomain(string domain)  //可测
        {
            //非空判断 正则验证
            if ("".Equals(domain))
            {
                return false;
            }
            else
            {
                try
                {
                    Regex reg = new Regex(@"^((25[0-5])|(2[0-4]\d)|(1\d\d)|([1-9]\d)|\d)(\.((25[0-5])|(2[0-4]\d)|(1\d\d)|([1-9]\d)|\d)){3}$");
                    Boolean _domaincheck = reg.IsMatch(domain);
                    //Boolean _ipcheck = Regex.IsMatch(domain, "^(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|[1-9])\\."
                    //                        + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)\\."
                    //                        + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)\\."
                    //                        + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)$");//是ip
                    //Regex rg = new Regex("^[\u4e00-\u9fa5]+$");//是中文
                    //Boolean _domaincheck = rg.IsMatch(domain);
                    if (_domaincheck)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 将用户信息变成Json数据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>Json数据字符串</returns>
        private string GetJson(string username,string password)
        {
            Dictionary<string, string> protocolIdentification = new Dictionary<string, string>();
            protocolIdentification.Add("useaname", Username);
            protocolIdentification.Add("password", Password);
            return JsonConvert.SerializeObject(protocolIdentification);
        }
        /// <summary>
        /// 将record信息变成Json数据，只有DNS站点调用
        /// </summary>
        /// <param name="lookup">被解析的站点域名</param>
        /// <param name="password">可能的结果</param>
        /// <returns>Json数据字符串</returns>
        private string GetJson(int type, string lookup, string expectedResults)
        {
            Dictionary<string, string> protocolIdentification = new Dictionary<string, string>();
            switch (type)
            {
                case 0:
                    protocolIdentification.Add("recordType", "A");
                    break;
                case 1:
                    protocolIdentification.Add("recordType", "CNAME");
                    break;
                case 2:
                    protocolIdentification.Add("recordType", "NS");
                    break;
                case 3:
                    protocolIdentification.Add("recordType", "MX");
                    break;
                default:
                    break;
            }
            protocolIdentification.Add("lookup", lookup);
            protocolIdentification.Add("expectedResults", expectedResults);
            return JsonConvert.SerializeObject(protocolIdentification);
        }

        /// <summary>
        /// 将int型的站点协议转换为string
        /// </summary>
        /// <param name="type">int型的站点协议</param>
        /// <returns>string型的站点协议</returns>
        private string GetProtocolType(int type)
        {
            string str = "ICMP";
            switch (type)
            {
                case 0:
                    str = "ICMP";
                    break;
                case 1:
                    str = "SOCKET";
                    break;
                case 2:
                    str = "SSH";
                    break;
                case 3:
                    str = "FTP";
                    break;
                case 4:
                    str = "DNS";
                    break;
                case 5:
                    str = "SMTP";
                    break;
                default:
                    break;
            }
            return str;
        }

        /// <summary>
        /// 将string型的站点协议转换为int，与界面的下拉列表对应
        /// </summary>
        /// <param name="type">string型的站点协议</param>
        /// <returns>int型的站点协议</returns>
        private int GetProtocolType(string type)
        {
            int i = 0;
            switch (type)
            {
                case "ICMP":
                    i = 0;
                    break;
                case "SOCKET":
                    i = 1;
                    break;
                case "SSH":
                    i = 2;
                    break;
                case "FTP":
                    i = 3;
                    break;
                case "DNS":
                    i = 4;
                    break;
                case "SMTP":
                    i = 5;
                    break;
                default:
                    break;
            }
            return i;
        }

        /// <summary>
        /// 将string型的RecordType转换为int, 与界面的下拉列表对应
        /// </summary>
        /// <param name="type">string型的RecordType</param>
        /// <returns>int型的RecordType</returns>
        private int GetRecordType(string type)
        {
            int i = 0;
            switch (type)
            {
                case "A":
                    i = 0;
                    break;
                case "CNAME":
                    i = 1;
                    break;
                case "NS":
                    i = 2;
                    break;
                case "MX":
                    i = 3;
                    break;
                default:
                    break;
            }
            return i;
        }

        /// <summary>
        /// 返回原界面
        /// </summary>
        private void Jump()
        {
            if (page == 1)
            {
                NavigationService.Navigate(typeof(Views.MainPage));
            }
            if (page == 2)
            {
                NavigationService.Navigate(typeof(Views.AllServer));
            }
        }
        #endregion
    }
}
