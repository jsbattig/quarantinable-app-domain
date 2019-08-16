using Ascentis.Framework;

namespace QuarantinableAppDomainLibTests
{
    [QuarantinableWrapper]
    public class TestWrapperClass : BaseWrapperClass
    {
        private dynamic _wrapperObject;

        public TestWrapperClass(QuarantinableAppDomain appDomain) : base(appDomain)
        {
            _wrapperObject = appDomain.CreateInstance("TestOffendingCppLib.TesterClass");
        }

        public bool SelfTest()
        {
            return _wrapperObject.SelfTest();
        }

        public void ThrowAccessViolation()
        {
            _wrapperObject.ThrowAccessViolation();
        }

        public int SelfTestCallsCount()
        {
            return _wrapperObject.SelfTestCallsCount();
        }
    }
}
