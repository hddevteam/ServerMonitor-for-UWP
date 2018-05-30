using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Services.RequestServices;
using System.Net;

namespace TestServerMonitor.TestRequest
{
    [TestClass]
    public class TestFTPRequest
    {
        private static FTPRequest request = new FTPRequest(LoginType.Identify);
        private const string TestFTPIP = "47.94.251.85";
        private bool result = false;

        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {
            request = new FTPRequest(LoginType.Identify)
            {
                FtpServer = IPAddress.Parse(TestFTPIP)
            };
        }

        /**
             * 测试用例：正常ftp服务器，正常用户名密码，特定用户的登入模式
             * 测试输入：Username = free，Password = free，Server = 47.94.251.85
             * 预期输出：MakeRequest() = true; request.ProtocalInfo != null
             */
        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Priority(1)]
        [Timeout(5000)]        
        public void TestMakeRequest_CorrectlyLogin()
        {
            request.Identification.Username = "free";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsTrue(result, "正常成功 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "正常成功 用例 请求协议内容为空！");                     
        }

        /**
             * 测试用例：服务器无ftp功能模块，正常用户名密码，特定用户的登入模式
             * 测试输入：Username = free，Password = free，Server = 8.8.8.8
             * 预期输出：MakeRequest() = false; request.ProtocalInfo != null
             */
        /// <summary>
        /// 测试输入IP为非FTP服务器的IP
        /// </summary>
        [TestMethod]
        [Owner("Bin")]
        [Priority(3)]
        //[Timeout(5000)]
        public void TestMakeRequest_ServerNotFTP()
        {           
            request.FtpServer = IPAddress.Parse("8.8.8.8");
            request.Identification.Username = "free";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsFalse(result, "服务器无ftp功能模块 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "服务器无ftp功能模块 用例 请求协议内容为空！");
        }

        /**
             * 测试用例：正常ftp服务器，用户名与密码不正确，特定用户|匿名 的登入模式
             * 测试输入：Username = free，Password = test，Server = 47.94.251.85
             * 预期输出：MakeRequest() = false; request.ProtocalInfo != null
             */
        /// <summary>
        /// 测试用户与密码不匹配
        /// </summary>
        [TestMethod]
        [Owner("Bin")]
        [Priority(2)]
        [Timeout(5000)]
        public void TestMakeRequest_UsernameOrPassError()
        {            
            request.Identification.Username = "123";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsFalse(result, "用户名与密码不正确 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "用户名与密码不正确 用例 请求协议内容为空！");
        }

        [TestMethod]
        [Owner("Bin")]
        [Priority(2)]
        [Timeout(5000)]
        public void TestMakeRequest_UsernameOrPassNull()
        {
            request.Identification.Username = null;
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsFalse(result, "用户名与密码不正确 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "用户名与密码不正确 用例 请求协议内容为空！");
        }


    }
}
