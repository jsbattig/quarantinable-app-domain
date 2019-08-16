using System;
using System.Reflection;
using System.Threading;

namespace Ascentis.Framework
{
    public class QuarantinableAppDomain
    {
        private ReaderWriterLock _currentAppDomainLock = new ReaderWriterLock();
        public AppDomainWrapper CurrentAppDomainWrapper
        {
            get
            {
                _currentAppDomainLock.AcquireReaderLock(-1);
                try
                {
                    if (_currentAppDomainWrapper != null) 
                        return _currentAppDomainWrapper;
                    var lockCookie =_currentAppDomainLock.UpgradeToWriterLock(-1);
                    try
                    {
                        _currentAppDomainWrapper = new AppDomainWrapper(Name, this);
                    }
                    finally
                    {
                        _currentAppDomainLock.DowngradeFromWriterLock(ref lockCookie);
                    }
                    return _currentAppDomainWrapper;
                }
                finally
                {
                    _currentAppDomainLock.ReleaseReaderLock();
                }
            }
        }

        public string Name { get; }
        private volatile AppDomainWrapper _currentAppDomainWrapper;

        public QuarantinableAppDomain(string libName)
        {
            Name = libName;
        }

        public bool IsQuarantinableException(Exception e)
        {
            return e is AccessViolationException;
        }

        public void AppDomainWrapperCompromised(AppDomainWrapper appDomainWrapper)
        {
            if (appDomainWrapper != _currentAppDomainWrapper) return;
            _currentAppDomainWrapper = null;
        }

        public void UnloadCurrentAppDomain()
        {
            if (_currentAppDomainWrapper == null) 
                return;
            _currentAppDomainLock.AcquireWriterLock(-1);
            try
            {
                _currentAppDomainWrapper.Unload();
                _currentAppDomainWrapper = null;
            }
            finally
            {
                _currentAppDomainLock.ReleaseWriterLock();
            }
        }

        public object CreateInstance(string className)
        {
            _currentAppDomainLock.AcquireReaderLock(-1);
            try
            {
                return CurrentAppDomainWrapper.LinkedDomain.CreateInstanceAndUnwrap(Name, className);
            }
            finally
            {
                _currentAppDomainLock.ReleaseReaderLock();
            }
        }
    }
}
