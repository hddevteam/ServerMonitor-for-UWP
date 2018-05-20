using Microsoft.QueryStringDotNET;
using Microsoft.Toolkit.Uwp.Notifications;
using ServerMonitor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace ServerMonitor.Util
{
	class MessageRemind
	{
		public void ShowToast(SiteModel site)
		{
			
			string title = "Server Monitor";
			string content = GetContent(site);
			//string image = "ms-appx:///Images/error.png";
			string logo = "ms-appx:///Images/ic_logo.png";

			ToastVisual visual = new ToastVisual()
			{
				BindingGeneric = new ToastBindingGeneric()
				{
					Children =
					{
						new AdaptiveText()
						{
							Text = title
						},

						new AdaptiveText()
						{
							Text = content
						},

						//new AdaptiveImage()
						//{
						//	Source = image
						//}
					},

					AppLogoOverride = new ToastGenericAppLogo()
					{
						Source = logo,
						HintCrop = ToastGenericAppLogoCrop.Circle
					}
				}
			};
			ToastActionsCustom actions = new ToastActionsCustom()
			{
				Buttons =
				{
					new ToastButton("OK", new QueryString()
					{}.ToString())
					{
						ActivationType = ToastActivationType.Background,
					},
				}
			};

			// Now we can construct the final toast content
			ToastContent toastContent = new ToastContent()
			{
				Visual = visual,
				Actions = actions,

				//// Arguments when the user taps body of toast
				//Launch = new QueryString()
				//{
				//	//{ "action", "viewConversation" },
				//	//{ "conversationId", conversationId.ToString() }

				//}.ToString()
			};
			ToastNotification notification = new ToastNotification(toastContent.GetXml());
			// And then send the toast
			ToastNotificationManager.CreateToastNotifier().Show(notification);
		}

		/// <summary>
		/// 通过站点生成错误字符串
		/// </summary>
		/// <param name="siteModel">发生错误的站点</param>
		/// <returns></returns>
		private String GetContent(SiteModel siteModel )
		{
			int id = siteModel.Id;//站点id
			int time = siteModel.Request_interval;//时间
			string name = siteModel.Site_name;//站点name
			var code = siteModel.Status_code;//状态码
			string content = "#" + id +" "+ name + " Error in " + time + " ms";
			return content;
		}
	}
}
