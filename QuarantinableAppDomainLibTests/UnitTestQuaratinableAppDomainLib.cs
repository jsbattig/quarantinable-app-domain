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
        private QuarantinableAppDomain qap;

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

            if (qap != null)
            {
                qap.UnloadCurrentAppDomain();
                qap = null;
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
            dynamic obj = qap.CurrentAppDomainWrapper.LinkedAssembly.CreateInstance("TestOffendingCppLib.TesterClass");
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestCreateWrapperClass()
        {
            qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            var obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
        }

        [TestMethod]
        public void TestCreateWrapperClassAndSelfTest()
        {
            Internal_TestCreateWrapperClassAndSelfTest();
        }

        private void Internal_TestCreateWrapperClassAndSelfTest()
        {
            qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            Assert.IsNotNull(qap.CurrentAppDomainWrapper);
            var obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.SelfTest());
            obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.SelfTest());
        }

        [TestMethod]
        public void TestCreateWrapperClassAndFailCallingUnloadedAppDomain()
        {
            qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            Assert.IsNotNull(qap.CurrentAppDomainWrapper);
            var obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
            Assert.IsTrue(obj.SelfTest());
            qap.UnloadCurrentAppDomain();
            try
            {
                obj.SelfTest();
                Assert.Fail("Should have error out after unloading appDomain");
            }
            catch (AppDomainUnloadedException)
            {
            }
        }

        [TestMethod]
        public void TestCreateWrapperClassAndSelfTestTwice()
        {
            Internal_TestCreateWrapperClassAndSelfTest();
            Internal_TestCreateWrapperClassAndSelfTest();
        }

        [TestMethod]
        public void TestCreateWrapperClassAndCallQuarantinableMethod()
        {
            qap = new QuarantinableAppDomain("TestOffendingCppLib");
            Assert.IsNotNull(qap);
            var obj = new TestWrapperClass(qap);
            var startInstanceCounter = AppDomainWrapper.InstanceCounter;
            Assert.IsNotNull(obj);
            Assert.AreEqual(startInstanceCounter, AppDomainWrapper.InstanceCounter);
            var initialSelfTestCallCounter = obj.SelfTestCallsCount();
            try
            {
                obj.SelfTest();
                Assert.AreEqual(initialSelfTestCallCounter + 1, obj.SelfTestCallsCount());
                obj.SelfTest();
                Assert.AreEqual(initialSelfTestCallCounter + 2, obj.SelfTestCallsCount());
                obj.ThrowAccessViolation();
                Assert.Fail("Should have thrown quarantinable Exception");
            }
            catch (QuarantinableException)
            {
            }
            qap.UnloadCurrentAppDomain();
            obj = new TestWrapperClass(qap);
            Assert.IsNotNull(obj);
            Assert.AreNotEqual(startInstanceCounter, AppDomainWrapper.InstanceCounter);
            obj.SelfTest();
            Assert.AreEqual(1, obj.SelfTestCallsCount());
        }
    }
}
