using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Diagnostics;
using ServerMonitor.Services.RequestServices;
using System;

namespace TestServerMonitor.TestRequest
{
    [TestClass]
    public class TestDnsRequest// Dns请求模块测试
    {
        private static IPAddress testIpAddress = IPAddress.Parse("8.8.8.8");
        private static string testDomainName = "www.baidu.com";
        private DnsRequest dnsRequest = new DnsRequest(testIpAddress, testDomainName);

        [Owner("Bin")]
        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {
            Debug.WriteLine("TestInitialize 方法调用");
        }



        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_NormallySucceed()// 测试发起Dns请求 
        {
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);

            Assert.AreEqual(true, dnsRequest.MakeRequest().Result, "测试 成功请求case 解析不成功!");
            Assert.AreEqual("1000", dnsRequest.Status, "测试 成功请求case 状态码不为正常状态码!");
        }

        /// <summary>
        /// 测试输入的dnsServer URL为空
        /// </summary>
        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_ServerIPNull()// 测试发起Dns请求 
        {
            dnsRequest.DnsServer = null;
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);

            Assert.IsFalse(dnsRequest.MakeRequest().Result, "测试 URL为空case 异常解析通过!");
            Assert.AreEqual("1001", dnsRequest.Status, "测试 URL为空case 状态码不为1001!");
        }

        /// <summary>
        /// 测试Dns请求解析的域名为空
        /// </summary>
        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_TestDomainNameValueNull()// 测试发起Dns请求 
        {
            dnsRequest.DomainName = "";
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);

            Assert.IsFalse(dnsRequest.MakeRequest().Result, "测试 DomainName为空case 异常解析通过!");
            Assert.AreEqual("1001", dnsRequest.Status, "测试 DomainName为空case 状态码不为1001!");
        }

        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_TimeOut()// 测试发起Dns请求 
        {
            dnsRequest.OverTime = 1;
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            dnsRequest.MakeRequest().Wait();
            //Assert.IsFalse(dnsRequest.MakeRequest().Result, "测试 请求超时case 异常解析通过!");
            //Assert.AreEqual("1002", dnsRequest.Status, "测试 请求超时case 状态码不为1002!");
        }

        [Owner("Bin")]
        [TestMethod]
        [Priority(2)]
        public void TestIsMatchResult()
        {
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            // 校验解析是否成功
            dnsRequest.MakeRequest().Wait();
            Assert.IsNotNull(dnsRequest.ActualResult);
            //Assert.AreEqual(true, dnsRequest.IsMatchResult("180.149.131.98", dnsRequest.ActualResult), "解析结果不匹配！");
        }
    }
}
