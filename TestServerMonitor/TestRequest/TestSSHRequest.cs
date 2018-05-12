using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Controls;
using ServerMonitor.Services.RequestServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServerMonitor.TestRequest
{
    [TestClass]
    public class TestSSHRequest
    {
        //测试信息
        private string TestSshIP = "172.31.0.244";
        private SSHRequest sshRequest;

        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {
            sshRequest = new SSHRequest(TestSshIP, SshLoginType.Identify);
        }
        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void CompleteAndCorrectInfo_ShouldReturnTrueAndProtocolInfoIsNull()
        {
            sshRequest.Identification.Username = "root";
            sshRequest.Identification.Password = "Lucky.2011";
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsTrue(actual);
            Assert.IsNull(sshRequest.ProtocolInfo);
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void UsernameOrPassError_ShouldReturnFalseAndProtocolInfoNotNull()
        {
            sshRequest.Identification.Username = "error";
            sshRequest.Identification.Password = "Lucky.2011";
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsFalse(actual);
            Assert.IsNotNull(sshRequest.ProtocolInfo);
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void ServerNotSsh_ShouldReturnFalseAndProtocolInfoNotNull()
        {
            sshRequest.IPAddress = "8.8.8.8";
            sshRequest.Identification.Username = "root";
            sshRequest.Identification.Password = "Lucky.2011";
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsFalse(actual);
            Assert.IsNotNull(sshRequest.ProtocolInfo);
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void ServerIsInvalid_ShouldReturnFalseAndProtocolInfoNotNull()
        {
            sshRequest.IPAddress = "111.2.3.4";
            sshRequest.Identification.Username = "root";
            sshRequest.Identification.Password = "Lucky.2011";
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsFalse(actual);
            Assert.IsNotNull(sshRequest.ProtocolInfo);
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void ServerIsEmpty_ShouldReturnFalseAndProtocolInfoNotNull()
        {
            sshRequest.IPAddress = "";
            sshRequest.Identification.Username = "root";
            sshRequest.Identification.Password = "Lucky.2011";
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsFalse(actual);
            Assert.IsNotNull(sshRequest.ProtocolInfo);
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void UsernameIsEmpty_ShouldThrowAggregateException()
        {
            sshRequest.IPAddress = "172.31.0.244";
            sshRequest.Identification.Username = "";
            sshRequest.Identification.Password = "Lucky.2011";
            try
            {
                var actual = sshRequest.MakeRequest().Result;
                Assert.Fail();
            }
            catch (AggregateException)
            {

            }
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void ShouldThrowAggregateExceptionWheniPAddressOrUsernameOrPassIsNull()
        {
            sshRequest.IPAddress = "172.31.0.244";
            sshRequest.Identification.Username = "root";
            sshRequest.Identification.Password = null;

            try
            {
                var actual = sshRequest.MakeRequest().Result;
                Assert.Fail();
            }
            catch (AggregateException)
            {

            }
        }
    }
}
