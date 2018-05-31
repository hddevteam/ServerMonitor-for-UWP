using Heijden.DNS;
using ServerMonitor.Services.RequestServices;
using ServerMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
    public sealed partial class AddServerPage : Page
    {
        AddServerPageViewModel model;
        public AddServerPage()
        {
            this.InitializeComponent();
            this.Loaded += AddServerPage_Loaded;
        }
        private void AddServerPage_Loaded(object sender, RoutedEventArgs e)
        {
            model = this.ViewModel as AddServerPageViewModel;
            model.OnLoaded(contactList, this.RightFrame1);
        }
        /// <summary>
        /// LiuYang 2018/5/27
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestServerConnection(object sender,RoutedEventArgs e)
        {
            switch (model.ProtocolType) {
                case 0://ICMP
                    SendICMPRequest();
                    break;
                case 1://SOCKET
                    SendSocketRequest();
                    break;
                case 2://SSH
                    SendSSHRequest();
                    break;
                case 3://FTP
                    SendFTPRequest();
                    break;
                case 4://DNS
                    SendDNSRequestAsync();
                    break;
                case 5://SMTP
                    SendSMTPRequest();
                    break;
                default:
                    break;

            }
        }
        /// <summary>
        /// 发送测试SMTP请求
        /// </summary>
        private async void SendSMTPRequest()
        {
            int port;
            if (!int.TryParse(model.Port, out port)) {//检测端口号是否为书字
                await new MessageDialog("the port is not a number").ShowAsync();
            }
            SMTPRequest request = new SMTPRequest(model.SiteAddress,port);
            await request.MakeRequest();
            await new MessageDialog(request.Status).ShowAsync();//显示返回状态
        }
        /// <summary>
        /// 发送测试FTP请求
        /// </summary>
        private async void SendFTPRequest()
        {
            IPAddress address = null;
            try//检测IP是否合法
            {
                address = IPAddress.Parse(model.SiteAddress);
            }
            catch
            {
                await new MessageDialog("wrong address").ShowAsync();
                return;
            }
            IdentificationInfo userInfo = new IdentificationInfo();//用户输入信息
            userInfo.Username = model.Username;
            userInfo.Password = model.Password;
            LoginType loginType = LoginType.Anonymous;
            if (model.NeedUser)//检测登录方式
            {
                loginType = LoginType.Identify;
            }
            FTPRequest request = new FTPRequest(loginType);
            request.Identification = userInfo;
            request.IdentifyType = loginType;
            request.FtpServer = address;
            await request.MakeRequest();
            await new MessageDialog(request.ProtocalInfo).ShowAsync();//显示返回信息
        }
        /// <summary>
        /// 发送测试SSH请求
        /// </summary>
        private async void SendSSHRequest()
        {
            SshIdentificationInfo userInfo = new SshIdentificationInfo();//登录信息
            userInfo.Username = model.Username;
            userInfo.Password = model.Password;
            SshLoginType loginType = SshLoginType.Anonymous;
            if (model.NeedUser)//检测登录方式
            {
                loginType = SshLoginType.Identify;
            }
            else
            {
                loginType = SshLoginType.Anonymous;
            }
            SSHRequest request = new SSHRequest(model.SiteAddress,loginType);
            request.Identification = userInfo;
            Task<bool>result=  request.MakeRequest();
            await result;
            await new MessageDialog(request.ProtocolInfo).ShowAsync();//显示返回结果
        }
        /// <summary>
        /// 发送测试ICMP请求
        /// </summary>
        private async void SendICMPRequest()
        {
            IPAddress address;
            try//检测IP是否合法
            {
                address = IPAddress.Parse(model.SiteAddress);
            }
            catch
            {
                await new MessageDialog("It's a wrong Address").ShowAsync();
                return;
            }
            ICMPRequest request = new ICMPRequest(address);
            request.MakeRequest();
            List<RequestObj> objs = request.Requests;
            foreach (RequestObj tmp in objs)//ICMPRequest一共发送五条请求，其中有一条有问题就返回错误信息
            {
                if (tmp.Status.Equals("1000"))
                {
                    continue;
                }
                else
                {
                    await new MessageDialog("error:" + tmp.Status).ShowAsync();
                    return;
                }
            }
            if (objs.Count == 5)//如果请求的个数不为5，并且所有请求都正常说明发生了未知的错误
            {
                await new MessageDialog("success!").ShowAsync();
            }
            else
            {
                await new MessageDialog("has a unknown error").ShowAsync();
            }
        }
        /// <summary>
        /// 发送测试DNS请求
        /// </summary>
        private async void  SendDNSRequestAsync()
        {
            DNSRequest request = DNSRequest.Instance;
            IPAddress server;
            try//检测IP是否合法
            {
                server = IPAddress.Parse(model.SiteAddress);
            }
            catch
            {
                await new MessageDialog("SiteAddress is wrong").ShowAsync();
                return;
            }
            QType type=QType.A;//默认值为A
            switch (model.RecordType) {//获取QType
                case 0:
                    type = QType.A;
                    break;
                case 1:
                    type = QType.CNAME;
                    break;
                case 2:
                    type = QType.NS;
                    break;
                case 3:
                    type = QType.MX;
                    break;
                default:
                    break;
            }
            request.DomainName = model.Lookup;
            request.RecordType = type;
            request.DnsServer = server;
            await request.MakeRequest();
            await new MessageDialog(request.RequestInfos).ShowAsync();//显示返回信息
        }
        /// <summary>
        /// 发送测试Socket请求
        /// </summary>
        private async void SendSocketRequest()
        {
            SocketRequest request = new SocketRequest();
            IPAddress address = null;
            try//检测IP是否合法
            {
                address = IPAddress.Parse(model.SiteAddress);
            } catch {
                await new MessageDialog("It's a wrong Address").ShowAsync();
                return;
            }
            int port = 0;
            if (!int.TryParse(model.Port, out port))//检测端口号是否为数字
            {
                await new MessageDialog("the port is wrong").ShowAsync();
            }
            IPEndPoint iPEndPoint = new IPEndPoint(address, port);
            request.TargetEndPoint = iPEndPoint;
            await request.MakeRequest();
            await new MessageDialog(request.ProtocolInfo).ShowAsync();//显示返回信息
        }
    }
}
