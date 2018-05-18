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
using Windows.ApplicationModel.Background;
using ServerMonitor.SiteDb;
using ServerMonitor.Models;
using ServerMonitor.Controls;


namespace ServerMonitor.Views
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
			//这个计时器用于定时请求pre-check 每逢分钟的0 5进行一次请求，数据放入数据库中
			DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };//倒计时间隔1s
			timer.Tick += new EventHandler<object>(async (sender, e) =>
			{
				await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(async () =>
				{
					//https://github.com/hddevteam/ServerMonitor-for-UWP.git
					var time = DateTime.Now.Minute;
					var s = DateTime.Now.Second;
					if (time % 5 == 0 && s == 0)
					{
                        MainPageViewModel model = this.ViewModel as MainPageViewModel;
                        bool flag = await model.Pre_Check();//执行pre check
						int text = 0;
						cdtxt.Text = text.ToString("00:00");
                    }
					else
					{                    
                        var min = DateTime.Now.Minute;
						var sec = DateTime.Now.Second;
						var dmin = 4 - (min % 5);
						var dsec = 60 - sec;
						if (dsec == 60)
						{
							dsec = 0;
							dmin = dmin + 1;
						}
						cdtxt.Text = (dmin).ToString("00") + ":" + (dsec).ToString("00");
					}
				}));
			});
			timer.Start();
            
            //在这里启动后台定时十五分钟请求

            var  builder = new BackgroundTaskBuilder();
			builder.Name = "backgroud request";//进程内的name 用于标识任务内容即可
			builder.SetTrigger(new TimeTrigger(15, false));//用法在官方文档表明，15表示间隔，不能更少，否则会抛出异常，false代表不是一次
			BackgroundTaskRegistration taskRegistration = builder.Register();//注册后台服务
			//int i = 0;
			//int TimeCount = 300;//倒计时秒数
			//DispatcherTimer timer = new DispatcherTimer() { Interval = new TimeSpan(0, 0, 1) };//倒计时间隔1s
			//timer.Tick += new EventHandler<object>(async (sender, e) =>
			//{
			//    await Dispatcher.TryRunAsync(CoreDispatcherPriority.Normal, new DispatchedHandler(() =>
			//    {
			//        //https://github.com/hddevteam/ServerMonitor-for-UWP.git
			//        i += 1;
			//        //double temp = (90 * Math.PI) * i / TimeCount / 10;                   
			//        cdtxt.Text = ((TimeCount - i) / 60).ToString("00") + ":" + ((TimeCount - i) % 60).ToString("00");
			//        if (i == TimeCount)
			//        {
			//            MainPageViewModel.Pre_Check();//执行pre check
			//            i = 0;
			//        }
			//    }));
			//});
			//timer.Start();
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