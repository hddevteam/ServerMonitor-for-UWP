using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Controls;
using ServerMonitor.DAOImpl;
using ServerMonitor.Models;
using ServerMonitor.Services.RequestServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
        int page = 1;  //1 MainPage, 2 AllServerPage，3 SiteDetail
        int siteId = -1;  //-1没有id是新建site  ，不为-1代表Edit那个站点

        private string _Value = "Default";  //保存传过来的信息  为 "page,siteId"
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }
        private bool contactChange = false;  //true 绑定联系人改变了
        #endregion

        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();
            await Task.CompletedTask;
            await Task.Run(()=> {    // OnNavigatedToAsync为异步方法，与OnLoaded谁先谁后不一定 把数据从Value中解析出来
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
                    MyTitle = "Edit Server";
                    CanDelete = true;
                    GetEditSite();  //需要id 放这里
                    SetVS();
                }
                else
                {
                    MyTitle = "Add Server";
                    CanDelete = false;
                }
            });

            //根据vs刷新选中联系人 相当于初始化SelectedContacts 放这才有vs为true的数据
            for (int i = 0; i < Contacts.Count; i++)
            {
                if (vs[Contacts[i].Id])
                {
                    SelectedContacts.Add(Contacts[i]);
                }
            }
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
        private bool canDelete;
        public bool CanDelete
        {
            get => canDelete;
            set
            {
                canDelete = value;
                RaisePropertyChanged(() => CanDelete);
            }
        }

        private bool noAnonymous = false;  //true 用户登陆  false：匿名
        public bool NoAnonymous
        {
            get => noAnonymous;
            set
            {
                noAnonymous = value;
                RaisePropertyChanged(() => NoAnonymous);
            }
        }
        private bool anonymous = true;  //true 匿名  false：用户登陆
        public bool Anonymous
        {
            get => anonymous;
            set
            {
                anonymous = value;
                RaisePropertyChanged(() => Anonymous);
            }
        }
        private bool livePort;  //Port是否可人为输入 true 可输入
        public bool LivePort
        {
            get => livePort;
            set
            {
                livePort = value;
                RaisePropertyChanged(() => LivePort);
            }
        }
        private bool diePort;  // = ! livePort
        public bool DiePort
        {
            get => diePort;
            set
            {
                diePort = value;
                RaisePropertyChanged(() => DiePort);
            }
        }

        private bool needUser;  //是否需要用户名和密码
        public bool NeedUser
        {
            get => needUser;
            set
            {
                needUser = value;
                RaisePropertyChanged(() => NeedUser);
            }
        }

        private bool needRecord;   //是否需要Record
        public bool NeedRecord
        {
            get => needRecord;
            set
            {
                needRecord = value;
                RaisePropertyChanged(() => NeedRecord);
            }
        }

        private bool isEnabled;   //save按钮是否可用
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                RaisePropertyChanged(() => IsEnabled);
            }
        }
        #endregion
        private string myTitle;
        public string MyTitle
        {
            get => myTitle;
            set
            {
                myTitle = value;
                RaisePropertyChanged(() => MyTitle);
            }
        }

        private ObservableCollection<ContactModel> contacts = new ObservableCollection<ContactModel>(); //所有联系人，在本界面只添加一次数据
        public ObservableCollection<ContactModel> Contacts { get => contacts; set => contacts = value; }

        private ObservableCollection<ContactModel> selectedContacts = new ObservableCollection<ContactModel>();  //选中的绑定联系人
        public ObservableCollection<ContactModel> SelectedContacts { get => selectedContacts; set => selectedContacts = value; }

        //协议类型，在下面的辅助方法GetProtocolType(int type)里转换
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

        private string port;
        public string Port
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
                    tempVs[Contacts[i].Id] = vs[Contacts[i].Id];  //vs->tempVs 在tempVs上更改数据
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
        /// 侧边栏Ok按钮点击事件 保存对绑定联系人的编辑
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
            contactChange = true;
        }
        
        /// <summary>
        /// 联系人列表item点击事件
        /// </summary>
        public void Contactlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = (ContactModel)e.ClickedItem;
            tempVs[item.Id] = !tempVs[item.Id];
        }

        /// <summary>
        /// 判断Domain是否合法
        /// </summary>
        public void Domain_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool flag = false;
            var textBox = sender as TextBox;
            if (textBox.Tag.Equals("1"))  //检查站点地址
            {
                string domain = textBox.Text.ToString();
                flag = CheckDomain(domain);
                //domain没问题了，看看Port有没有问题 另：只有0:ICMP，1:SOCKET需要手动输入Port，需判断 --xn
                if (flag && (ProtocolType == 0 || ProtocolType == 1))
                {
                    flag = CheckPort(Port);
                }
            }
            else   //检查端口
            {
                string port = textBox.Text.ToString();
                flag = CheckPort(port);
                if (flag) //Port没问题了，看看domain有没有问题 --xn
                {
                    flag = CheckDomain(SiteAddress);
                }
            }
            IsEnabled = flag;
        }
        
        /// <summary>
        /// 上传提交，保存到数据库。改用异步方法 -xn
        /// </summary>
        public async void SaveAsync()
        {
            if (!(await CheckSite(SiteAddress)))
            {
                IsEnabled = false;
                return;
            }
            if (siteId == -1)  //新建Site
            {
                SaveAdd();
            }
            else      //Edit site
            {
                SaveEdit();
            }
        }
        /// <summary>
        /// 新建保存
        /// </summary>
        public void SaveAdd()
        {
            SiteModel site = new SiteModel()
            {
                Is_server = true,
                Monitor_interval = 5,
                Is_Monitor = true,
                Create_time = DateTime.Now,
                Update_time = DateTime.Now,
                Is_success = 2,
                Request_succeed_code = "1000",
            };

            //将界面数据保存下来
            GetUIDate(site);

            //生成可存进数据库的list数据
            List<SiteContactModel> contactSiteModels = new List<SiteContactModel>();
            foreach (var item in vs)  
            {
                if (item.Value)
                {
                    contactSiteModels.Add(new SiteContactModel()
                    {
                        SiteId = siteId,
                        ContactId = item.Key,
                    });
                }
            }
            //数据库操作
            if (DBHelper.InsertOneSite(site) == 1)
            {
                var contactS = ContactSiteDAOImpl.Instance.InsertListConnects(contactSiteModels);
                Jump(); //返回原界面
            }
        }
        /// <summary>
        /// 编辑保存
        /// </summary>
        public void SaveEdit()
        {
            SiteModel site = DBHelper.GetSiteById(siteId);
            site.Protocol_content = null;
            site.ProtocolIdentification = null;
            site.Update_time = DateTime.Now;
            
            //生成可存进数据库的list数据
            List<SiteContactModel> contactSiteModels = new List<SiteContactModel>();
            foreach (var item in vs)
            {
                if (item.Value)
                {
                    contactSiteModels.Add(new SiteContactModel()
                    {
                        SiteId = siteId,
                        ContactId = item.Key,
                    });
                }
            }
            //将界面数据保存下来 因为传入的是引用类型，所以无需返回值
            GetUIDate(site);
            //数据库操作
            if (DBHelper.UpdateSite(site) == 1)
            {
                if (contactChange)
                {
                    var in1 = ContactSiteDAOImpl.Instance.DeletSiteAllConnect(siteId);
                    var contactS = ContactSiteDAOImpl.Instance.InsertListConnects(contactSiteModels);
                }
                Jump();
            }
        }

        /// <summary>
        /// 将界面数据保存下来
        /// </summary>
        /// <param name="site">保存在该站点里</param>
        public void GetUIDate(SiteModel site)
        {
            site.Protocol_type = GetProtocolType(ProtocolType);
            site.Site_address = SiteAddress;
            try
            {
                site.Server_port = int.Parse(Port);
            }
            catch (Exception)
            {
                site.Server_port = 0;
            }
            if (SiteName == null || SiteName.Equals(""))
            {
                site.Site_name = SiteAddress;
            }
            else
            {
                site.Site_name = SiteName;
            }
            if (site.Protocol_type.Equals("SSH") || site.Protocol_type.Equals("FTP"))
            {
                if (NoAnonymous) //true 不匿名 用户请求
                {
                    site.ProtocolIdentification = GetJson(Username, Password, "1");
                }
                else  //匿名
                {
                    site.ProtocolIdentification = GetJson("", "", "0");
                }
            }
            else if (site.Protocol_type.Equals("DNS"))
            {
                site.Protocol_content = GetJson(RecordType, Lookup, ExpectedResults);
            }
        }

        /// <summary>
        /// 删除站点，只在编辑站点时用到
        /// </summary>
        public async void DeleteSiteAsync()
        {
            string str = SiteName + " will be deleted.";
            var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Delete this server?" };
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("OK", uicommand =>
            {
                if (DBHelper.DeleteOneSite(siteId) == 1)
                {
                    if (page == 1)
                    {
                        NavigationService.Navigate(typeof(Views.MainPage));
                    }
                    else if (page == 2)
                    {
                        NavigationService.Navigate(typeof(Views.AllServerPage));
                    }
                    else if (page == 3)
                    {
                        NavigationService.Navigate(typeof(Views.MainPage));
                    }
                }
            }));
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", uicommand =>
            {

            }));
            await messageBox.ShowAsync();
        }

        /// <summary>
        /// 取消修改/添加 返回原界面
        /// </summary>
        public void CancelBack()
        {
            Jump();
        }

        public void Anonymous_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox.IsChecked == true) //匿名
            {
                NoAnonymous = false;
            }
            else
            {
                NoAnonymous = true;
            }
        }
        public void GetExpected_Click()
        {

        }
        #endregion

        #region 异步辅助函数
        /// <summary>
        /// 最后判断，判断网站地址是否可以解析
        /// </summary>
        /// <returns>是否可以解析</returns>
        public async Task<bool> CheckSite(string domain)
        {
            try
            {
                if (!IPAddress.TryParse(domain, out IPAddress reIP))
                {
                    var http = domain.StartsWith("http://");
                    var https = domain.StartsWith("https://");
                    if (http)
                    {
                        domain = domain.Substring(7);//去除http://
                    }
                    else if (https)
                    {
                        domain = domain.Substring(8);//去除https://
                    }

                    IPAddress[] hostEntry = await Dns.GetHostAddressesAsync(domain);
                }
                else
                {
                    IPAddress.Parse(domain);
                }
            }
            catch (Exception)
            {
                //不可解析 弹出对话框
                IsEnabled = false;
                string str = "Unable to parse the domain name you entered!!!!";  //弹出框文本
                var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Warning" };
                messageBox.Commands.Add(new Windows.UI.Popups.UICommand("OK", uicommand =>
                {
                }));
                await messageBox.ShowAsync();  //弹出框显示
                return false;
            }
            return true;
        }
        #endregion

        #region 辅助函数
        /// <summary>
        /// OnNavigatedTo后调用 UI控件对象传递 界面元素可交互
        /// </summary>
        public void OnLoaded(ListView contactList,Grid grid)
        {
            rightFrame1 = grid;   //侧边栏
            this.contactList = contactList;   //侧边栏联系人列表
            rightFrame1.Visibility = Visibility.Collapsed;  //关闭侧边栏
            IsEnabled = (CheckDomain(SiteAddress) && CheckPort(Port));  //初始判断 Sava是否可用
        }

        /// <summary>
        /// 获取联系人列表 初始化变量 只调用一次
        /// </summary>
        private void GetListContact()  //不可测
        {
            List<ContactModel> list = DBHelper.GetAllContact();
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
                    Port = null;
                    break;
                case 1:     //SOCKET
                    LivePort = true;
                    DiePort = false;
                    NeedUser = false;
                    NeedRecord = false;
                    Port = null;
                    break;
                case 2:    //SSH
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    Port = "22";
                    break;
                case 3:   //FTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    Port = "21";
                    break;
                case 4:    //DNS
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = true;
                    Port = "53";
                    break;
                case 5:    //SMTP
                    LivePort = true;
                    DiePort = false;
                    NeedUser = false;
                    NeedRecord = false;
                    Port = "25";
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
            SiteModel site = DBHelper.GetSiteById(siteId);
            ProtocolType = GetProtocolType(site.Protocol_type);
            SiteAddress = site.Site_address;
            SiteName = site.Site_name;
            Port = site.Server_port+"";

            if (ProtocolType == 2 || ProtocolType == 3)
            {
                JObject js = (JObject)JsonConvert.DeserializeObject(site.ProtocolIdentification);
                if (js["type"].ToString().Equals("1"))  //用户请求
                {
                    Username = js["username"].ToString();//用户名
                    Password = js["password"].ToString();//用户名
                    NoAnonymous = true;
                    Anonymous = false;
                }
                else
                {
                    NoAnonymous = false;
                    Anonymous = true;
                }
            }
            else if(ProtocolType == 4)
            {
                JObject js = (JObject)JsonConvert.DeserializeObject(site.Protocol_content);
                RecordType = GetRecordType(js["recordType"].ToString());
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
            if ("".Equals(domain) || domain == null)
            {
                return false;
            }
            else
            {
                try
                {
                    //ip的正则表达式
                    Regex regIP = new Regex(@"^((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$");
                    bool _ipcheck = regIP.IsMatch(domain);

                    //域名的正则表达式
                    var http = domain.StartsWith("http://");
                    var https = domain.StartsWith("https://");
                    if (http)
                    {
                        domain = domain.Substring(7);//去除http://
                    }
                    else if (https)
                    {
                        domain = domain.Substring(8);//去除https://
                    }

                    Regex reg = new Regex(@"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?$");
                    bool _domaincheck = reg.IsMatch(domain);
                    if (_ipcheck || _domaincheck)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 检验站点端口是否合法
        /// </summary>
        /// <param name="port">站点端口</param>
        /// <returns>是否合法</returns>
        private bool CheckPort(string port)
        {
            if (port == null)
            {
                return false;
            }
            Regex regPort = new Regex(@"^[1-9][0-9]{0,5}$");
            bool check = regPort.IsMatch(port);
            if (check)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 将用户信息变成Json数据
        /// </summary>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <returns>Json数据字符串</returns>
        private string GetJson(string username,string password,string type)
        {
            if (username == null)
            {
                username = "";
            }
            if (password == null)
            {
                password = "";
            }
            Dictionary<string, string> protocolIdentification = new Dictionary<string, string>
            {
                { "username", Username },
                { "password", Password },
                { "type", type }
            };
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
            else if (page == 2)
            {
                NavigationService.Navigate(typeof(Views.AllServerPage));
            }
            else if (page == 3)
            {
                NavigationService.Navigate(typeof(Views.SiteDetailPage), siteId);
            }
        }

        /// <summary>
        /// 在Edit时，vs填充数据
        /// </summary>
        private void SetVS()
        {
            var contactS = ContactSiteDAOImpl.Instance.GetConnectsBySiteId(siteId);
            for (int i = 0; i < contactS.Count; i++)  //vs填充数据
            {
                var q = (from t in Contacts
                        where t.Id == contactS[i].ContactId
                        select t).ToList().Count;
                if (q > 0)
                {
                    vs[contactS[i].ContactId] = true;
                }
            }
        }
        #endregion
    }
}
