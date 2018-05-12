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
    public sealed partial class ContactPage : Page
    {
        ContactPageViewModel model;
        public ContactPage()
        {
            this.InitializeComponent();
            this.Loaded += Contact_Loaded;
        }

        private void Contact_Loaded(object sender, RoutedEventArgs e)
        {
            model = this.ViewModel as ContactPageViewModel;
            model.SetFrame(this.RightFrame1,this.RightFrame2);
        }

        private void ContactList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if(model==null)
            {
                model = this.ViewModel as ContactPageViewModel;
            }
            if (model.ContactList_RightTapped(sender, e))  //交由ViewModel中的对应的方法处理
            {
                contactListFlyout.ShowAt(contactList, e.GetPosition(this.contactList)); //弹出框显示
            }
        }
    }
}
