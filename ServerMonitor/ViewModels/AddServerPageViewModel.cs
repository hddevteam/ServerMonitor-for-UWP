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
        private ListView contactList;
        private Grid rightFrame1;
        Dictionary<int,bool> vs = new Dictionary<int, bool>(); //记录绑定联系人的结果
        Dictionary<int, bool> tempVs = new Dictionary<int, bool>(); //记录绑定联系人过程中的选择变化
        public AddServerPageViewModel() //先-> ComboBox_SelectionChanged-> OnNavigatedToAsync-> OnLoaded
        {
            
        }
        private string _Value = "Default";
        public string Value { get { return _Value; } set { Set(ref _Value, value); } }

        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            ChangeDisplay(0);
            GetListContact();
            Value = (suspensionState.ContainsKey(nameof(Value))) ? suspensionState[nameof(Value)]?.ToString() : parameter?.ToString();
            await Task.CompletedTask;
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

        private Site editSite = new Site();
        public Site EditSite { get => editSite; set => editSite = value; }

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
            if(EditSite!=null)
            {
                EditSite.Protocol_type = ((ComboBoxItem)((sender as ComboBox).SelectedItem)).Content.ToString();
            }
        }
        /// <summary>
        /// BindContact按钮点击事件 呼出侧边栏
        /// </summary>
        public void BindContact_Click(object sender, RoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Visible;
            for (int i = 0; i < contactList.Items.Count; i++)
            {
                //在contactList的item里添加新控件时在这一步报错时，关闭vs，再打开看看
                ((ListViewItem)contactList.ContainerFromIndex(i)).IsSelected = vs[Contacts[i].Id];//根据vs设置contactList选中效果
                tempVs[Contacts[i].Id] = vs[Contacts[i].Id];
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

            //非空判断 正则验证
            if ("".Equals(domain))
            {
                IsEnabled = false;
            }
            else
            {
                try
                {
                    Boolean _ipcheck = Regex.IsMatch(domain, "^(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|[1-9])\\."
                                            + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)\\."
                                            + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)\\."
                                            + "(1\\d{2}|2[0-4]\\d|25[0-5]|[1-9]\\d|\\d)$");//是ip
                    Regex rg = new Regex("^[\u4e00-\u9fa5]+$");//是中文
                    Boolean _domaincheck = rg.IsMatch(domain);
                    if (!_ipcheck && _domaincheck)
                    {
                        IsEnabled = false;
                    }
                    else
                    {
                        IsEnabled = true;
                    }
                }
                catch { }
            }
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
            GetEditSite();  //需要id 放这里
        }

        /// <summary>
        /// 获取联系人 初始化变量 只调用一次
        /// </summary>
        private void GetListContact()  //不可测
        {
            List<Contact> list = DBHelper.GetAllContact();
            Contacts.Clear();
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
                    break;
                case 3:   //FTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = true;
                    NeedRecord = false;
                    break;
                case 4:    //DNS
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = true;
                    break;
                case 5:    //SMTP
                    LivePort = false;
                    DiePort = true;
                    NeedUser = false;
                    NeedRecord = false;
                    break;
                default:
                    break;
            }
        }

        private void GetEditSite()
        {
            //EditSite.Protocol_type = "ICMP";
        }
        #endregion
    }
}
