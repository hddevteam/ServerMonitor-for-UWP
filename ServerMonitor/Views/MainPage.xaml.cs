using System;
using ServerMonitor.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Xaml.Input;
using System.Linq;
using Windows.UI.Core;

namespace ServerMonitor.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
            int i = 0;
            int TimeCount = 300;//倒计时秒数
            DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };//倒计时间隔1s
            timer.Tick += new EventHandler<object>(async (sender, e) =>
            {
                await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
                {
                    //https://github.com/hddevteam/ServerMonitor-for-UWP.git
                    i += 1;
                    //double temp = (90 * Math.PI) * i / TimeCount / 10;                   
                    cdtxt.Text = ((TimeCount - i) / 60).ToString("00") + ":" + ((TimeCount - i) % 60).ToString("00");
                    if (i == TimeCount)
                    {
                        MainPageViewModel.Pre_Check();//执行pre check
                        i = 0;
                    }
                }));
            });
            timer.Start();

            this.Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //交由ViewModel中的对应的方法处理
            (this.ViewModel as MainPageViewModel).Loaded(this.termsOfUseContentDialog);
        }

        /// <summary>
        /// 站点列表右击列表 得到右击的站点id
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void SiteList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MainPageViewModel model = this.ViewModel as MainPageViewModel;
            if (model.SiteList_RightTapped(sender, e)) //交由ViewModel中的对应的方法处理
            {
                site_flyout.ShowAt(sitelist, e.GetPosition(this.sitelist));
            }
        }
    }
}