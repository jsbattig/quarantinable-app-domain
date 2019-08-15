using System;
using System.IO;
using Ascentis.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace QuarantinableAppDomainLibTests
{
    [TestClass]
    public class UnitTestQuaratinableAppDomainLib
    {
        private static readonly string WindowsFolder = Environment.ExpandEnvironmentVariables("%windir%");
        private AppDomain _appDomain;

        private void CreateAppDomain()
        {
            var appDomainSetup = new AppDomainSetup()
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };
            _appDomain = AppDomain.CreateDomain("AppD1", AppDomain.CurrentDomain.Evidence, appDomainSetup);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (_appDomain != null)
            {
                AppDomain.Unload(_appDomain);
                _appDomain = null;
            }
        }

        [TestMethod]
        public void TestVerifyMachineConfig()
        {
            var x64SubPath = IntPtr.Size == 4 ? "" : "64";
            var machineConfig = File.ReadAllText($@"{WindowsFolder}\Microsoft.NET\Framework{x64SubPath}\v4.0.30319\Config\machine.config");
            Assert.IsTrue(machineConfig.Contains("<legacyCorruptedStateExceptionsPolicy enabled=\"true\" />"));
        }

        [TestMethod]
        public void TestLoadLibDefaultAppDomainAndSelfTest()
        {
            var assembly = AppDomain.CurrentDomain.Load("TestOffendingCppLib");
            dynamic obj = assembly.CreateInstance("TestOffendingCppLib.TesterClass");
            Assert.IsNotNull(obj);
            obj.SelfTest();
        }

        [TestMethod]
        public void TestLoadLibDefaultAppDomainAndThrowAccessViolation()
        {
            var assembly = AppDomain.CurrentDomain.Load("TestOffendingCppLib");
            dynamic obj = assembly.CreateInstance("TestOffendingCppLib.TesterClass");
            Assert.IsNotNull(obj);
            try
            {
                obj.ThrowAccessViolation();
                Assert.Fail("Should throw Access Violation");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(AccessViolationException));
            }
        }

        [TestMethod]
        public void TestLoadLibNewAppDomainAndSelfTest()
        {
            CreateAppDomain();
            var assembly = _appDomain.Load("TestOffendingCppLib");
            dynamic obj = assembly.CreateInstance("TestOffendingCppLib.TesterClass");
            Assert.IsNotNull(obj);
            obj.SelfTest();
        }

        [TestMethod]
        public void TestLoadLibNewAppDomainAndThrowAccessViolation()
        {
            CreateAppDomain();
            var assembly = _appDomain.Load("TestOffendingCppLib");
            dynamic obj = assembly.CreateInstance("TestOffendingCppLib.TesterClass");
            Assert.IsNotNull(obj);
            try
            {
                obj.ThrowAccessViolation();
                Assert.Fail("Should throw Access Violation");
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(AccessViolationException));
            }
        }

        [TestMethod]
        public void TestCreateQuarantinableAppDomain()
        {
            var qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
        }

        [TestMethod]
        public void TestQuarantinableAppDomainCreateInstance()
        {
            var qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            dynamic obj = qap.CreateInstance("TestOffendingCppLib.TesterClass");
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestCreateWrapperClass()
        {
            var qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            var obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestCreateWrapperClassAndSelfTest()
        {
            var qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            var obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.SelfTest());
        }
    }
}
