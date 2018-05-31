using ServerMonitor.Services.RequestServices;
using ServerMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class AddWebsitePage : Page
    {
        AddWebsitePageViewModel model;
        public AddWebsitePage()
        {
            this.InitializeComponent();
            this.Loaded += AddWebsitePage_Loaded;
        }

        private void AddWebsitePage_Loaded(object sender, RoutedEventArgs e)
        {
            model = this.ViewModel as AddWebsitePageViewModel;
            model.OnLoaded(contactList, this.RightFrame1);
        }
        private void TestSiteConnection(object sender, RoutedEventArgs e)
        {
            CheckHttpRequestAsync();
        }
        /// <summary>
        /// liuyang 2018/5/27
        /// 发送HTTP测试请求
        /// </summary>
        private async void CheckHttpRequestAsync()
        {
            HTTPRequest request = HTTPRequest.Instance;
            request.Status = null;//HTTPrequest在发送请求中发生异常时未给Status赋值，由于是单例模式会保存上次请求结果 设为null防止显示错误结果
            request.Uri = ProtocolType.SelectionBoxItem.ToString() + model.SiteAddress;
            Task<bool> result = request.MakeRequest();
            await result;
            bool test = result.Result;
            if (request.Status != null)//如果返回不为空说明请求过程没有问题，显示返回码和耗时
            {
                await new MessageDialog(request.Status + "\t耗时" + request.TimeCost).ShowAsync();
            }
            else
            {
                await new MessageDialog("域名错误或不存在").ShowAsync();
            }
        }
    }
}
