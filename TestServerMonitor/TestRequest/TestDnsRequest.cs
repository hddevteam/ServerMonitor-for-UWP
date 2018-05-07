using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServerMonitor.Controls;
using System.Net;
using System.Diagnostics;

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
        public void TestMakeRequest()// 测试发起Dns请求 
        {
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            Assert.AreEqual(true, dnsRequest.MakeRequest().Result, "解析不成功!");
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
            Assert.AreEqual(true, dnsRequest.IsMatchResult("180.149.131.98", dnsRequest.ActualResult), "解析结果不匹配！");
        }
    }
}
