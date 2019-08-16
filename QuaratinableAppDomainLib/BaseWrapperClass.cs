using NConcern;

namespace Ascentis.Framework
{
    public class BaseWrapperClass
    {
        public AppDomainWrapper LinkedAppDomainWrapper { get; }

        static BaseWrapperClass()
        {
            Aspect.Weave<QuarantinableWrapperAttribute>(typeof(QuarantinableWrapperAttribute));
        }

        public BaseWrapperClass(QuarantinableAppDomain linkedQuarantinableAppDomainAppDomain)
        {
            LinkedAppDomainWrapper = linkedQuarantinableAppDomainAppDomain.CurrentAppDomainWrapper;
        }
    }
}
