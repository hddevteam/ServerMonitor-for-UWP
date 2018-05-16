using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerMonitor.Controls;
using ServerMonitor.DAOImpl;
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
    class AddWebsitePageViewModel: ViewModelBase,INotifyPropertyChanged
    {
        public AddWebsitePageViewModel()
        {
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
            await Task.Run(() => {    // OnNavigatedToAsync为异步方法，与OnLoaded谁先谁后不一定 把数据从Value中解析出来
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
                    SetVS();
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

        private ObservableCollection<ContactModel> contacts = new ObservableCollection<ContactModel>(); //所有联系人，在本界面只添加一次数据
        public ObservableCollection<ContactModel> Contacts { get => contacts; set => contacts = value; }

        private ObservableCollection<ContactModel> selectedContacts = new ObservableCollection<ContactModel>();  //选中的绑定联系人
        public ObservableCollection<ContactModel> SelectedContacts { get => selectedContacts; set => selectedContacts = value; }

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
        
        private string statusCodes;
        public string StatusCodes
        {
            get => statusCodes;
            set
            {
                statusCodes = value;
                RaisePropertyChanged(() => StatusCodes);
            }
        }
        //PRIORITY
        private int priority; //现在没用到
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
            }
            else   //检查状态码
            {
                string codes = textBox.Text.ToString();
                flag = CheckCodes(codes);
            }
            IsEnabled = flag;
        }

        /// <summary>
        /// 上传提交，保存到数据库
        /// </summary>
        public void Save()
        {
            SiteModel site;
            if (siteId == -1)  //新建Site
            {
                site = new SiteModel()
                {
                    Is_server = false,
                    Monitor_interval = 5,
                    Is_Monitor = true,
                    Server_port = 1,
                    Create_time = DateTime.Now,
                    Update_time = DateTime.Now,
                    Last_request_result = 2,  //代表unknown
                    //Status_code = "1000/0",
                    Request_succeed_code = "200",
                };
            }
            else    //Edit site
            {
                site = DBHelper.GetSiteById(siteId);
                site.Update_time = DateTime.Now;
            }
            
            //将界面数据保存下来
            site.Protocol_type = GetProtocolType(ProtocolType);
            site.Site_address = (ProtocolType == 0 ? "http://" : "https://") + SiteAddress;
            site.Status_code = "200," + StatusCodes;

            if (SiteName == null || SiteName.Equals(""))
            {
                site.Site_name = site.Site_address;
            }
            else
            {
                site.Site_name = SiteName;
            }

            //生成可存进数据库的绑定联系人list数据
            List<ContactSiteModel> contactSiteModels = new List<ContactSiteModel>();
            foreach (var item in vs)
            {
                if (item.Value)
                {
                    contactSiteModels.Add(new ContactSiteModel()
                    {
                        SiteId = siteId,
                        ContactId = item.Key,
                    });
                }
            }
            //数据库操作
            if (siteId == -1)
            {
                if (DBHelper.InsertOneSite(site) == 1)
                {
                    var contactS = ContactSiteDAOImpl.Instance.InsertListConnects(contactSiteModels);
                    Jump(); //返回原界面
                }
            }
            else
            {
                if (DBHelper.UpdateSite(site) == 1)
                {
                    var in1 = ContactSiteDAOImpl.Instance.DeletSiteAllConnect(siteId);
                    var contactS = ContactSiteDAOImpl.Instance.InsertListConnects(contactSiteModels);
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
        /// OnNavigatedTo后调用 UI控件对象传递 界面元素可交互
        /// </summary>
        public void OnLoaded(ListView contactList, Grid grid)
        {
            rightFrame1 = grid;   //侧边栏
            this.contactList = contactList;   //侧边栏联系人列表
            rightFrame1.Visibility = Visibility.Collapsed;  //关闭侧边栏
            IsEnabled = CheckDomain(SiteAddress);  //检验设置Sava是否可用
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

        //在siteId!=-1时调用， 即Edit site 时调用
        /// <summary>
        /// Edit site 时调用 将数据库数据分配到绑定字段里
        /// </summary>
        private void GetEditSite()
        {
            SiteModel site = DBHelper.GetSiteById(siteId);
            ProtocolType = GetProtocolType(site.Protocol_type);
            if (ProtocolType == 0)
            {
                SiteAddress = site.Site_address.Substring(7);
            }
            else
            {
                SiteAddress = site.Site_address.Substring(8);
            }
            SiteName = site.Site_name;
            if (site.Status_code.Length>=5)
            {
                StatusCodes = site.Status_code.Substring(4);

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
        /// 检验状态码是否合法
        /// </summary>
        /// <param name="codes">状态码字符串</param>
        /// <returns>是否合法</returns>
        private bool CheckCodes(string codes)  //可测
        {
            if (codes.Equals("")||codes==null)
            {
                return true;
            }
            string[] arr = codes.Split(',');
            for (int i = 0; i < arr.Count(); i++)
            {
                try
                {
                    int x = int.Parse(arr[i]);
                }
                catch (Exception)
                {

                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 将int型的站点协议转换为string
        /// </summary>
        /// <param name="type">int型的站点协议</param>
        /// <returns>string型的站点协议</returns>
        private string GetProtocolType(int type)
        {
            string str = "HTTP";
            switch (type)
            {
                case 0:
                    str = "HTTP";
                    break;
                case 1:
                    str = "HTTPS";
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
                case "HTTP":
                    i = 0;
                    break;
                case "HTTPS":
                    i = 1;
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

        /// <summary>
        /// 在Edit时，vs填充数据 要有siteId
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
