using ServerMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace ServerMonitor.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AddMonitor : Page
    {
        public AddMonitor()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;

            this.Loaded += OnLoaded;
        }

        //界面生成显示前运行
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AddMonitorViewModel model = this.ViewModel as AddMonitorViewModel;
            model.OnLoaded(monitorlist);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AddMonitorViewModel model = this.ViewModel as AddMonitorViewModel;
            model.OnNavigatedTo();
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// 添加服务器的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addServer_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// 添加website的事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addWebsite_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
