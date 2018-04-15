using GalaSoft.MvvmLight;
using ServerMonitor.Controls;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Template10.Mvvm;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace ServerMonitor.ViewModels
{
    public class AddMonitorViewModel : Template10.Mvvm.ViewModelBase
    {
        private ListView monitorlist;
        private List<Site> sites;  //与绑定数据MonitorItems一一对应（第一个对应第一个）,只能在GetListMonitor（）增删操作
        public AddMonitorViewModel()
        {
            //GetListMonitor();
        }

        #region 系统函数
        //导航到该页面调用
        public void OnNavigatedTo()
        {
            GetListMonitor();
        }

        // OnNavigatedTo后调用
        public void OnLoaded(ListView monitorlist)
        {
            this.monitorlist = monitorlist;
            for (int i = 0; i < monitorlist.Items.Count; i++)
            {
                ((ListViewItem)monitorlist.ContainerFromIndex(i)).IsSelected = MonitorItems.ToList()[i].Is_Monitor;
            }
        }
        #endregion 系统函数

        #region 绑定数据
        private ObservableCollection<MonitorItem> monitorItems = new ObservableCollection<MonitorItem>();
        public ObservableCollection<MonitorItem> MonitorItems { get => monitorItems; set => monitorItems = value; }
        #endregion 绑定数据

        #region 响应事件
        //导航栏 完成按钮事件
        public void Accept_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < sites.Count; i++)
            {
                sites[i].Is_Monitor = MonitorItems.ToList()[i].Is_Monitor;
            }
            DBHelper.UpdateListSite(sites);
            NavigationService.Navigate(typeof(Views.MainPage));
        }

        //导航栏 取消按钮事件
        public void Cancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(typeof(Views.MainPage));
        }

        // 站点全选按钮事件
        public void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < monitorlist.Items.Count; i++)
            {
                ((ListViewItem)monitorlist.ContainerFromIndex(i)).IsSelected = true;
                MonitorItems.ToList()[i].Is_Monitor = true;
            }
        }

        //列表item点击事件
        public void Monitorlist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item =  (MonitorItem)e.ClickedItem;
            item.Is_Monitor = (!item.Is_Monitor);
            //int x = MonitorItems.IndexOf(item);
            //MonitorItem item = (MonitorItem)((ListViewItem)e.OriginalSource).DataContext;

        }

        //添加站点
        public void GotoAllserver() =>
            NavigationService.Navigate(typeof(Views.AllServer_addserver), 0);
        #endregion 响应事件

        #region 辅助函数
        private void GetListMonitor()
        {
            MonitorItems.Clear();
            sites = DBHelper.GetAllSite();
            for (int i = 0; i < sites.Count; i++)
            {
                if (sites[i].Is_pre_check)
                {
                    sites.Remove(sites[i]);
                    i--;
                    continue;
                }
                if (sites[i].Is_server)
                {
                    MonitorItems.Add(new MonitorItem()
                    {
                        Id = sites[i].Id,
                        Site_name = sites[i].Site_name,
                        Is_Monitor = sites[i].Is_Monitor,
                        Image = "/Images/ic_server.png",
                        Site_address=sites[i].Site_address
                    });
                }
                else
                {
                    MonitorItems.Add(new MonitorItem()
                    {
                        Id = sites[i].Id,
                        Site_name = sites[i].Site_name,
                        Is_Monitor = sites[i].Is_Monitor,
                        Image = "/Images/ic_website.png",
                        Site_address = sites[i].Site_address

                    });
                }
            }
        }
        #endregion 辅助函数
    }

    public class MonitorItem : ObservableObject
    {
        int id;
        string image;
        string site_name;
        string site_address;
        bool is_Monitor;

        public int Id
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChanged(() => Id);
            }
        }
        public string Image
        {
            get => image;
            set
            {
                image = value;
                RaisePropertyChanged(() => Image);
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
        public bool Is_Monitor
        {
            get => is_Monitor;
            set
            {
                is_Monitor = value;
            }
        }
        public MonitorItem()
        {
            Image = "/Images/ic_server.png";
            Site_address = "WWW.google.com";
            Is_Monitor = false;
        }
    }
}
