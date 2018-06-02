using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Services.NavigationService;
using Windows.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using ServerMonitor.Models;
using ServerMonitor.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;
using System;
using System.Text.RegularExpressions;

namespace ServerMonitor.ViewModels
{
    public class ContactPageViewModel : Template10.Mvvm.ViewModelBase
    {
        private ContactModel selectedContact; //点击或右击对应的联系人
        Grid rightFrame2, rightFrame1;  //右方隐藏控件 RightFrame1:联系人详细信息;  RightFrame2:编辑联系人，新建联系人
        int isAddContact = 0; //1:添加联系人 2：编辑联系人 0：什么都不做
        public ContactPageViewModel()
        {

        }
        #region 绑定数据
        //联系人列表
        private ObservableCollection<ContactModel> contacts = new ObservableCollection<ContactModel>();
        public ObservableCollection<ContactModel> Contacts { get => contacts; set => contacts = value; }

        //用于编辑联系人，新建联系人的绑定联系人
        public ContactModel RightContact { get => rightContact; set => rightContact = value; }
        private ContactModel rightContact = new ContactModel();

        //RightFrame2的标题
        private string rightFrame2Title = "New Contact";
        public string RightFrame2Title
        {
            get => rightFrame2Title;
            set
            {
                rightFrame2Title = value;
                RaisePropertyChanged(() => RightFrame2Title);
            }
        }
        #endregion 绑定数据

        #region 系统函数
        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> suspensionState)
        {
            GetListContact();
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

        #region 响应事件
        /// <summary>
        /// 联系人列表右击 取得点击对应的联系人信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public bool ContactList_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var x = e.OriginalSource;
            //根据点击的事件源 取对应的联系人信息
            if (x is Rectangle)
            {
                selectedContact = (ContactModel)(((Rectangle)(e.OriginalSource)).DataContext);
            }
            else if (x is TextBlock)
            {
                selectedContact = (ContactModel)(((TextBlock)(e.OriginalSource)).DataContext);
            }
            else if(x is Image)
            {
                selectedContact = (ContactModel)(((Image)(e.OriginalSource)).DataContext);
            }
            return true;
        }
        /// <summary>
        /// 联系人列表点击 取得点击的联系人
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Contactlist_Tapped(object sender, TappedRoutedEventArgs e)
        {
            rightFrame1.Visibility = Visibility.Collapsed;
            int x = (sender as ListBox).SelectedIndex;
            if (x >= 0)
            {
                selectedContact = (ContactModel)((sender as ListBox).Items[x]);
                rightFrame1.Visibility = Visibility.Visible; //让联系人详情rightFrame1显示
                rightFrame2.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 联系人列表右击 显示弹出框中新建联系人
        /// </summary>
        public void AddContact()
        {
            isAddContact = 1;
            RightFrame2Title = "New Contact";
            RightContact.Id = 0;
            RightContact.Telephone = "";
            RightContact.Contact_email = "";
            RightContact.Contact_name = "";
            rightFrame1.Visibility = Visibility.Collapsed;
            rightFrame2.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 联系人列表右击 显示弹出框中编辑联系人 联系人详情中编辑联系人
        /// </summary>
        public void EditFlyoutItem_Click()
        {
            isAddContact = 2;
            RightFrame2Title = "Edit Contact";
            RightContact.Id = selectedContact.Id;
            RightContact.Telephone = selectedContact.Telephone;
            RightContact.Contact_email = selectedContact.Contact_email;
            RightContact.Contact_name = selectedContact.Contact_name;
            rightFrame1.Visibility = Visibility.Collapsed;
            rightFrame2.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 新建或编辑联系人中的确认
        /// </summary>
        public async void ConfirmContact()
        {
            string str = JudgeInput(RightContact); //判断输入的联系人信息是否合法
            if (!str.Equals(""))  //true：表示不合法 弹出框提醒
            {
                var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Error" };
                messageBox.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
                await messageBox.ShowAsync();
                return;
            }
            if(isAddContact==1)  //新建联系人
            {
                RightContact.Create_time = DateTime.Now;
                RightContact.Update_time = DateTime.Now;
                if (DBHelper.InsertOneContact(RightContact) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    rightFrame2.Visibility = Visibility.Collapsed;
                }
            }
            else if(isAddContact==2)  //更新联系人
            {
                RightContact.Update_time = DateTime.Now;
                if (DBHelper.UpdateContact(RightContact) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    rightFrame2.Visibility = Visibility.Collapsed;
                }
            }
            GetListContact();  //刷新联系人列表
        }
        /// <summary>
        /// 新建或编辑联系人中的取消
        /// </summary>
        public void CancelContact()
        {
            rightFrame1.Visibility = Visibility.Collapsed;
            rightFrame2.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 联系人列表右击 显示弹出框中删除联系人
        /// </summary>
        public async void DeleteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            string str = selectedContact.Contact_name + " will be deleted.";  //弹出框文本
            var messageBox = new Windows.UI.Popups.MessageDialog(str) { Title = "Delete this contact?" };
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Delete", uicommand =>
            {    //点击Delete，删除联系人
                if (DBHelper.DeleteOneContact(selectedContact.Id) == 1)
                {
                    rightFrame1.Visibility = Visibility.Collapsed;
                    rightFrame2.Visibility = Visibility.Collapsed;
                    GetListContact();
                }
            }));
            messageBox.Commands.Add(new Windows.UI.Popups.UICommand("Cancel", uicommand =>
            {     //点击Cancel  不作为

            }));
            await messageBox.ShowAsync();  //弹出框显示
        }
        #endregion 响应事件

        #region 辅助函数
        /// <summary>
        /// 获取和刷新联系人
        /// </summary>
        private void GetListContact()  //不可测
        {
            List<ContactModel> list = DBHelper.GetAllContact();
            Contacts.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                Contacts.Add(list[i]);
            }
        }
        
        /// <summary>
        /// 设置，获取界面右端隐藏控件
        /// </summary>
        public void SetFrame(Grid grid1,Grid grid2)  //不可测
        {
            rightFrame1 = grid1;
            rightFrame2 = grid2;
        }

        /// <summary>
        /// 深度克隆联系人Contact 
        /// </summary>
        private ContactModel CloneContact(ContactModel contact)  //可测
        {
            ContactModel ct = new ContactModel()
            {
                Id = contact.Id,
                Contact_name = contact.Contact_name+"1",
                Contact_email = contact.Contact_email,
                Create_time = contact.Create_time,
                Telephone = contact.Telephone,
            };
            return ct;
        }
        /// <summary>
        /// 判断新建，编辑时输入是否合法
        /// </summary>
        private string JudgeInput(ContactModel contact) //可测
        {
            string str = "";
            //手机号码
            Regex regPho = new Regex(@"^(13[0-9]|14[5|7]|15[0|1|2|3|5|6|7|8|9]|17[0|6|7|8]|18[0-9])\d{8}$");

            //国内电话号码(0511-4405222、021-87888822)
            Regex regPho1 = new Regex(@"\d{3}-\d{8}|\d{4}-\d{7}");

            //Email地址
            Regex regE = new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");

            //如果两个都不不匹配执行
            if(!(regPho.IsMatch(contact.Telephone)|| regPho1.IsMatch(contact.Telephone)))
            {
                if (!("".Equals(contact.Telephone) || contact.Telephone == null))
                {
                    str += "The telephone number is error.";
                }
            }
            if(!regE.IsMatch(contact.Contact_email))
            {
                if (!("".Equals(contact.Contact_email) || contact.Contact_email == null))
                    str += "The E-mail is error.";
            }
            return str;
        }
        #endregion 辅助函数
    }
}
