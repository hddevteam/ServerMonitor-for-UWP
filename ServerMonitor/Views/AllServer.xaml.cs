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
    public sealed partial class AllServer : Page
    {
        public AllServer()
        {
            this.InitializeComponent();
            //NavigationCacheMode = NavigationCacheMode.Enabled;
            this.Loaded += OnLoaded;
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AllServerViewModel model = this.ViewModel as AllServerViewModel;
            model.SetFrame(this.RightFrame1);
        }
        
        public void List_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            AllServerViewModel model = this.ViewModel as AllServerViewModel;
            if (model.List_RightTapped(sender, e))
            {
                site_flyout.ShowAt(ServerList, e.GetPosition(this.ServerList));
            }
        }
    }
}