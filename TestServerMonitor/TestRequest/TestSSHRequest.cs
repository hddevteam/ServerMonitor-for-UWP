using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerMonitor.Controls;
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
        private string Username = "root";
        private string Password = "Lucky.2011";
        private SSHRequest sshRequest;

        [TestInitialize()] // 测试类生成预处理
        public void Initialize()
        {
            sshRequest = new SSHRequest(TestSshIP, Username, Password);
        }
        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void CompleteAndCorrectInfo_ShouldReturnTrueAndProtocolInfoIsNull()
        {
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
            sshRequest.UserName = "error";
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
            sshRequest.iPAddress = "8.8.8.8";
            sshRequest.UserName = "root";
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
            sshRequest.iPAddress = "1.2.3.4";
            sshRequest.UserName = "root";
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
            sshRequest.iPAddress = "";
            sshRequest.UserName = "root";
            var actual = sshRequest.MakeRequest().Result;

            Assert.IsFalse(actual);
            Assert.IsNotNull(sshRequest.ProtocolInfo);
        }

        /// <summary>
        /// 测试MakeRequest
        /// </summary>
        [TestMethod]
        public void ShouldThrowAggregateExceptionWhenUsernameOrPassIsEmpty()
        {
            sshRequest.UserName = "";
            sshRequest.PassWord = "";
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
        public void ShouldThrowAggregateExceptionWhenUsernameOrPassIsNull()
        {
            sshRequest.UserName = null;
            sshRequest.iPAddress = null;
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
