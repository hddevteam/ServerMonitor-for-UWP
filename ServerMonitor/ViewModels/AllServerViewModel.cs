using Template10.Mvvm;
using GalaSoft.MvvmLight;
using ServerMonitor.Controls;
using ServerMonitor.Models;
using ServerMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using ServerMonitor.Views;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Shapes;
using ServerMonitor.DAO;

namespace ServerMonitor.ViewModels
{
    class AllServerViewModel : Template10.Mvvm.ViewModelBase
    {
        private List<SiteModel> sites;
        private ServerItem ServerContext;
        Grid rightFrame1;
        private int order = 1;  //1:id As 2:id De 3:Al As 4:Al De
        private int filter = 2; //0:Error  1:Normal  2:All Servers
        public AllServerViewModel() { }
        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GetListServer();
            await Task.CompletedTask;
        }
        public override async Task OnNavigatedFromAsync(IDictionary<string, object> suspensionState, bool suspending)
        {
            await Task.CompletedTask;
        }
        public override async Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            await Task.CompletedTask;
        }
        #endregion 系统函数

        #region 绑定数据
        private ObservableCollection<ServerItem> serverItems = new ObservableCollection<ServerItem>();
        public ObservableCollection<ServerItem> ServerItems { get => serverItems; set => serverItems = value; }
        //编辑，新建绑定站点
        public ServerItem RightServer { get => rightServer; set => rightServer = value; }
        private ServerItem rightServer = new ServerItem();

        //站点绑定的联系人
        private ObservableCollection<ContactModel> bindingContact = new ObservableCollection<ContactModel>();
        public ObservableCollection<ContactModel> BindingContact { get => bindingContact; set => bindingContact = value; }

        private string openOrClose;//新建/编辑
        public string OpenOrClose
        {
            get => openOrClose;
            set
            {
                openOrClose = value;
                RaisePropertyChanged(() => OpenOrClose);
            }
        }
        #endregion 绑定数据
        
        #region 响应事件
        /// <summary>
        /// 进行筛选的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Filter_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menu = (MenuFlyoutItem)e.OriginalSource;
            filter = int.Parse(menu.Tag.ToString());
            GetListServer();
        }
        /// <summary>
        /// 进行排序的按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Order_Click(object sender, RoutedEventArgs e)
        {
            MenuFlyoutItem menu = (MenuFlyoutItem)e.OriginalSource;
            order = int.Parse(menu.Tag.ToString());
            GetListServer();
        }
        /// <summary>
        /// 站点列表右击列表 得到右击的站点id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool List_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                ServerContext = (ServerItem)((TextBlock)e.OriginalSource).DataContext;
            }
            else if (e.OriginalSource is ToggleSwitch)
            {
                ServerContext = (ServerItem)((ToggleSwitch)e.OriginalSource).DataContext;
            }
            else if (e.OriginalSource is Image)
            {
                ServerContext = (ServerItem)((Image)e.OriginalSource).DataContext;
            }
            else
            {
                ServerContext = (ServerItem)((Rectangle)e.OriginalSource).DataContext;
            }
            if (null != ServerContext)
            {
                if(ServerContext.Site_status.Equals("Enable"))
                {
                    OpenOrClose = "Disable";
                }
                else
                {
                    OpenOrClose = "Enable";
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// ListBox点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void List_Tapped(object sender, TappedRoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Collapsed;

            int x = (sender as ListBox).SelectedIndex;
            if (x >= 0)
            {
                ServerContext = ((ServerItem)((sender as ListBox).Items[x]));
                rightFrame1.Visibility = Visibility.Visible;
            }

            if (null != ServerContext)
            {
                if (ServerContext.Site_status.Equals("Enable"))
                {
                    OpenOrClose = "Disable";
                }
                else
                {
                    OpenOrClose = "Enable";
                }
            }

            BindingContact.Clear();
            var list = (new ContactDAOImpl()).GetContactModelsBySiteId(ServerContext.Site_id);
            foreach (var item in list)
            {
                BindingContact.Add(item);
            }
        }
        

        //add server点击事件
        public void Add_Server(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.AddServerPage), "2,-1"); //1MainPage, 2 AllServer; -1没有id是新建site
        }
        //add website点击事件
        public void Add_Website(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.AddWebsitePage), "2,-1"); //1MainPage, 2 AllServer; -1没有id是新建site
        }


        //进入详细页面
        public void DetailFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.SiteDetail), ServerContext.Site_id);
        }
        //进入编辑页面
        public void EditFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var site = DBHelper.GetSiteById(ServerContext.Site_id);
            if (site.Is_server)
            {
                NavigationService.Navigate(typeof(Views.AddServerPage), "2,"+ ServerContext.Site_id); //1MainPage, 2 AllServer; -1没有id是新建site
            }
            else
            {
                NavigationService.Navigate(typeof(Views.AddWebsitePage), "2," + ServerContext.Site_id); //1MainPage, 2 AllServer; -1没有id是新建site
            }
            rightFrame1.Visibility = Visibility.Collapsed;
        }
        //关闭Server
        public void ClosedFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var q1 = from t in sites
                     where t.Id == ServerContext.Site_id
                     select t;
            q1.First().Is_Monitor = !ServerContext.Is_Monitor; ;
            if (DBHelper.UpdateSite(q1.First()) == 1)
            {
                GetListServer();  //更新数据库site表对应的sites
            }
            rightFrame1.Visibility = Visibility.Collapsed;
        }
        //取消
        public void CancelServer()
        {
            rightFrame1.Visibility = Visibility.Collapsed;
        }
        //删除
        public async void Delete_Click(object sender, RoutedEventArgs e)
        {
            string str = ServerContext.Site_name + " will be deleted.";
            var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Delete this server?" };
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Delete", uicommand =>
            {
                if (DBHelper.DeleteOneSite(ServerContext.Site_id) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    GetListServer();
                }
            }));
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", uicommand =>
            {

            }));
            await messageBox.ShowAsync();
        }
        #endregion 响应事件

        #region 辅助函数
        private void GetListServer()
        {
            ServerItems.Clear();
            sites = DBHelper.GetAllSite();
            List<SiteModel> q = ProcessSite(sites);
            string site_status;

            for (int i = 0; i < q.Count; i++)
            {
                if (q[i].Is_Monitor)
                {
                    site_status = "Enable";
                }
                else
                {
                    site_status = "Disable";
                }

                if (q[i].Is_server) //服务器
                {
                    ServerItems.Add(new ServerItem()
                    {
                        Site_id = q[i].Id,
                        Site_name = q[i].Site_name,
                        Site_address = q[i].Site_address,
                        Site_status_codes = "--",
                        Is_Monitor = q[i].Is_Monitor,
                        Site_type = "Server",
                        Site_status = site_status,
                        Image_path = "/Images/ic_server.png",
                        Protocol_type = q[i].Protocol_type,
                    });
                }
                else
                {
                    ServerItems.Add(new ServerItem()
                    {
                        Site_id = q[i].Id,
                        Site_name = q[i].Site_name,
                        Site_address = q[i].Site_address,
                        Site_status_codes = q[i].Status_code,
                        Is_Monitor = q[i].Is_Monitor,
                        Site_type = "WebSite",
                        Site_status = site_status,
                        Image_path = "/Images/ic_website.png",
                        Protocol_type = q[i].Protocol_type,
                    });
                }
            }
        }
        private List<SiteModel> ProcessSite(List<SiteModel> list)
        {
            List<SiteModel> q;
            if (filter == 2)  //0:Error  1:Normal  2:All Servers,
            {
                q = (from t in sites
                     select t).ToList();
            }
            else if (filter == 1)//正常1
            {
                q = (from t in sites
                     where t.Is_Monitor == true
                     where t.Is_success == 1
                     select t).ToList();
            }
            else
            {
                q = (from t in sites
                     where t.Is_Monitor == true
                     where t.Is_success != 1
                     select t).ToList();
            }
            switch (order)  //1:id As 2:id De 3:Al As 4:Al De
            {
                case 1:
                    q = (from t in q
                         orderby t.Id ascending
                         select t).ToList();
                    break;
                case 2:
                    q = (from t in q
                         orderby t.Id descending
                         select t).ToList();
                    break;
                case 3:
                    q = (from t in q
                         orderby t.Site_name ascending
                         select t).ToList();
                    break;
                case 4:
                    q = (from t in q
                         orderby t.Site_name descending
                         select t).ToList();
                    break;
                default:
                    break;
            }

            // 筛选掉 pre-Check
            q = (from t in q
                 where t.Is_pre_check == false
                 select t).ToList();

            return q;
        }
        //设置，获取界面右端隐藏元素
        public void SetFrame(Grid grid1)
        {
            rightFrame1 = grid1;
        }
        #endregion 辅助函数
    }
    public class ServerItem : ObservableObject
    {
        int site_id;
        string site_name;
        string site_address;
        string site_status_codes;
        string image_path;
        string site_type;
        bool is_Monitor;
        string site_status;//站点开关状态
        string protocol_type;
        /// <summary>
        /// 站点协议类型
        /// </summary>
        public string Protocol_type
        {
            get => protocol_type;
            set
            {
                protocol_type = value;
                RaisePropertyChanged(() => Protocol_type);
            }
        }
        public string Site_status
        {
            get => site_status;
            set
            {
                site_status = value;
                RaisePropertyChanged(() => Site_status);
            }
        }
        public int Site_id
        {
            get => site_id;
            set
            {
                site_id = value;
                RaisePropertyChanged(() => Site_id);
            }
        }
        public string Site_name
        {
            get => site_name;
            set
            {
                site_name = value;
                RaisePropertyChanged(() => Site_name);
            }
        }
        public string Site_address
        {
            get => site_address;
            set
            {
                site_address = value;
                RaisePropertyChanged(() => Site_address);
            }
        }
        /// <summary>
        /// 站点上次请求返回码
        /// </summary>
        public string Site_status_codes
        {
            get => site_status_codes;
            set
            {
                site_status_codes = value;
                RaisePropertyChanged(() => Site_status_codes);
            }
        }
        public string Image_path
        {
            get => image_path;
            set
            {
                image_path = value;
                RaisePropertyChanged(() => Image_path);
            }
        }
        public bool Is_Monitor
        {
            get => is_Monitor;
            set
            {
                is_Monitor = value;
                value = is_Monitor;
            }
        }
        public string Site_type { get => site_type; set => site_type = value; }
    }
}