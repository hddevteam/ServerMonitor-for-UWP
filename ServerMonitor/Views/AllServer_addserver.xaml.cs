using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
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
    public sealed partial class AllServer_addserver : Page
    {
        public AllServer_addserver()
        {
            this.InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        //{
        //    var toggle = sender as ToggleSwitch;
        //    if (toggle.IsOn)
        //    {                
        //        this.serverGrid.Visibility = Visibility.Collapsed;
        //        this.WebsiteGrid.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        this.serverGrid.Visibility = Visibility.Visible;
        //        this.WebsiteGrid.Visibility = Visibility.Collapsed;
        //    }

        //}

        //private void addCodeBtn_Click(object sender, RoutedEventArgs e)
        //{
        //    //添加监听端口号
        //    ComboBox combo = new ComboBox();
        //    string codeText = "";
        //    codeText = this.addCodeStatusText.Text;
        //    this.StatusCodeCombox.Items.Add(codeText);
        //    this.addCodeStatusText.Text = "";
        //    onAddCode();

        //}
        private async void onAddCode()
        {
            MessageDialog dialog = new MessageDialog("添加完成");
            dialog.Commands.Add(new UICommand("确定", cmd => { }, commandId: 0));
            await dialog.ShowAsync();
        }

        private void addCodeStatusText_TextChanged(object sender, TextChangedEventArgs e)
        {
            //利用正则表达式只允许输入数字
            var textbox = (TextBox)sender;
            try
            {
                if (!Regex.IsMatch(textbox.Text, "^\\d*\\.?\\d*$") && textbox.Text != "")
                {
                    int pos = textbox.SelectionStart - 1;
                    textbox.Text = textbox.Text.Remove(pos, 1);
                    textbox.SelectionStart = pos;
                }
            }
            catch
            {

            }
        }
    }
}
