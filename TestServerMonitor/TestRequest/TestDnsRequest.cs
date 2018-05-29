using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Diagnostics;
using ServerMonitor.Services.RequestServices;
using System;

/// <summary>
/// 测试协议请求域   创建：xb 创建时间：2018/04
/// </summary>
namespace TestServerMonitor.TestRequest
{
    /// <summary>
    /// 测试DNS协议请求模块  创建人：xb 创建时间：2018/04
    /// </summary>
    [TestClass]
    public class TestDNSRequest// Dns请求模块测试
    {
        private static IPAddress testIpAddress = IPAddress.Parse("8.8.8.8");
        private static string testDomainName = "www.baidu.com";
        private DNSRequest dnsRequest = new DNSRequest(testIpAddress, testDomainName);

        /// <summary>
        /// 每个测试初始化时执行的方法
        /// </summary>
        [Owner("Bin")]
        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {
            Debug.WriteLine("TestInitialize 方法调用");
        }

        /// <summary>
        /// 测试MakeRequest，测试用例为正常登入的测试用例
        /// </summary>
        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_NormallySucceed()// 测试发起Dns请求 
        {
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);

            Assert.AreEqual(true, dnsRequest.MakeRequest().Result, "Cannot succeed in accessing the specified DNS server");
            Assert.AreEqual("1000", dnsRequest.Status, "Failed in getting the \'Succeed\' status code!");
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

            Assert.IsFalse(dnsRequest.MakeRequest().Result, "Unusually passed when the DNS server address is null!");
            Assert.AreEqual("1001", dnsRequest.Status, "Failed in getting the \'Error\' status code!");
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

            Assert.IsFalse(dnsRequest.MakeRequest().Result, "Unusually passed when the domain name is null !");
            Assert.AreEqual("1001", dnsRequest.Status, "Failed in getting the \'Error\' status code!");
        }

        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_TimeOut()// 测试发起Dns请求 
        {
            dnsRequest.OverTime = 0;
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            dnsRequest.MakeRequest().Wait();
            Assert.IsFalse(dnsRequest.MakeRequest().Result, "Unusually passed when it is TimeOver !");
            Assert.AreEqual("1002", dnsRequest.Status, "Failed in getting the \'OverTime\' status code!");
        }

        [Owner("Bin")]// Owner 标签标识测试方法创建者
        [TestMethod]
        [Priority(1)]// 优先级
        public void TestMakeRequest_WithoutSpecifiedResource()// 测试发起Dns请求 
        {
            dnsRequest.RecordType = Heijden.DNS.QType.CNAME;
            dnsRequest.DnsServer = IPAddress.Parse("8.8.8.8");
            dnsRequest.DomainName = "naotu.baidu.com";
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            dnsRequest.MakeRequest().Wait();
            Assert.IsFalse(dnsRequest.MakeRequest().Result, "Unusually passed when it failed in getting specified record !");
        }

        /// <summary>
        /// 测试成功命中
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Priority(2)]
        public void TestIsMatchResult_NormallySucceed()
        {
            dnsRequest.DomainName = "localhost";            
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            // 校验解析是否成功
            dnsRequest.MakeRequest().Wait();
            Assert.IsNotNull(dnsRequest.ActualResult);
            Assert.AreEqual(true, dnsRequest.IsMatchResult("127.0.0.1", dnsRequest.ActualResult), "Cannot match the result ");
        }

        /// <summary>
        /// 测试未能成功命中
        /// </summary>
        [Owner("Bin")]
        [TestMethod]
        [Priority(2)]
        public void TestIsMatchResult_CannotHitRightly()
        {
            dnsRequest.DomainName = "localhost";
            // 校验 dnsRequest 是否为空
            Assert.AreNotEqual(null, dnsRequest);
            // 校验解析是否成功
            dnsRequest.MakeRequest().Wait();
            Assert.IsNotNull(dnsRequest.ActualResult);
            Assert.IsFalse(dnsRequest.IsMatchResult("127.0.0.3", dnsRequest.ActualResult), "Unusually matching result !");
        }
    }
}
