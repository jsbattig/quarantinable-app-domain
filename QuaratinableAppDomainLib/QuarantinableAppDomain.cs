using System;
using System.Reflection;
using System.Threading;

namespace Ascentis.Framework
{
    public class QuarantinableAppDomain
    {
        public AppDomainWrapper CurrentAppDomainWrapper => _currentAppDomainWrapper;

        private readonly string _name;
        private volatile AppDomainWrapper _currentAppDomainWrapper;

        public QuarantinableAppDomain(string libName)
        {
            _name = libName;
            _currentAppDomainWrapper = new AppDomainWrapper(libName, this);
        }

        public bool IsQuarantinableException(Exception e)
        {
            return e is AccessViolationException;
        }

        public void AppDomainWrapperCompromised(AppDomainWrapper appDomainWrapper)
        {
            if (appDomainWrapper != _currentAppDomainWrapper) return;
            _currentAppDomainWrapper = new AppDomainWrapper(_name, this);
        }

        public void UnloadCurrentAppDomain()
        {
            _currentAppDomainWrapper.Unload();
            _currentAppDomainWrapper = null;
        }
    }
}
