using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TestServerMonitor.TestRequest
{
    [TestClass]
    public class TestFtpRequest
    {
        private static FTPRequest request = new FTPRequest(LoginType.Identify);


        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {

        }


        [TestMethod]
        public void TestMakeRequest1()
        {
            bool result = false;
            request = new FTPRequest(LoginType.Identify)
            {
                FtpServer = IPAddress.Parse("47.94.251.85")
            };
            request.Identification.Username = "free";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsTrue(result, "正常成功 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "正常成功 用例 请求协议内容为空！");
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        public void TestMakeRequest()
        {
            bool result = false;

            /**
             * 测试用例：正常ftp服务器，正常用户名密码，特定用户的登入模式
             * 测试输入：Username = free，Password = free，Server = 47.94.251.85
             * 预期输出：MakeRequest() = true; request.ProtocalInfo != null
             */
            request = new FTPRequest(LoginType.Identify)
            {
                FtpServer = IPAddress.Parse("47.94.251.85")
            };
            request.Identification.Username = "free";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsTrue(result, "正常成功 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "正常成功 用例 请求协议内容为空！");

            /**
             * 测试用例：ftp服务器不填写为空,正常用户名密码，特定用户的登入模式
             * 测试输入：Username = free，Password = free，Server = null
             * 预期输出：MakeRequest() = true; request.ProtocalInfo != null
             */
            //request = new FTPRequest(LoginType.Identify)
            //{
            //    FtpServer = IPAddress.Parse("")
            //};
            //request.Identification.Username = "free";
            //request.Identification.Password = "free";
            //result = request.MakeRequest().Result;

            //Assert.IsFalse(result, "ftp服务器名称为空 用例 请求异常！");
            //Assert.IsNotNull(request.ProtocalInfo, "ftp服务器名称为空 用例 请求协议内容为空！");

            /**
             * 测试用例：服务器无ftp功能模块，正常用户名密码，特定用户的登入模式
             * 测试输入：Username = free，Password = free，Server = 8.8.8.8
             * 预期输出：MakeRequest() = false; request.ProtocalInfo != null
             */
            request = new FTPRequest(LoginType.Identify)
            {
                FtpServer = IPAddress.Parse("8.8.8.8")
            };
            request.Identification.Username = "free";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsFalse(result, "服务器无ftp功能模块 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "服务器无ftp功能模块 用例 请求协议内容为空！");

            /**
             * 测试用例：正常ftp服务器，用户名与密码不正确，特定用户|匿名 的登入模式
             * 测试输入：Username = free，Password = test，Server = 47.94.251.85
             * 预期输出：MakeRequest() = false; request.ProtocalInfo != null
             */
            request = new FTPRequest(LoginType.Identify)
            {
                FtpServer = IPAddress.Parse("47.94.251.85")
            };
            request.Identification.Username = "free";
            request.Identification.Password = "free";
            result = request.MakeRequest().Result;

            Assert.IsFalse(result, "用户名与密码不正确 用例 请求异常！");
            Assert.IsNotNull(request.ProtocalInfo, "用户名与密码不正确 用例 请求协议内容为空！");

            /**
             * 测试用例：正常ftp服务器，正常用户名密码，特定用户的登入模式，请求超时
             * 测试输入：Username = free，Password = free，Server = 47.94.251.85
             * 预期输出：MakeRequest() = false; request.ProtocalInfo != null
             */

        }
    }
}
