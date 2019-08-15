using System;
using System.Reflection;
using System.Threading;

namespace Ascentis.Framework
{
    public class QuarantinableAppDomain
    {
        private bool _quarantined;
        private volatile int _counter;
        private AppDomain _appDomain;
        private Assembly _assembly;
        private readonly string _name;

        private void BuildNewAppDomain()
        {
            var appDomainSetup = new AppDomainSetup()
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };
            _appDomain = AppDomain.CreateDomain(_name + _counter, AppDomain.CurrentDomain.Evidence, appDomainSetup);
            _assembly = _appDomain.Load(_name);
        }

        public void TraceableMethodEnter()
        {
            Interlocked.Increment(ref _counter);
        }

        public void TraceableMethodLeave()
        {
            Interlocked.Decrement(ref _counter);
        }

        public QuarantinableAppDomain(string libName)
        {
            _name = libName;
            BuildNewAppDomain();
        }

        public bool IsQuarantinableException(Exception e)
        {
            return e is AccessViolationException;
        }

        public object CreateInstance(string className)
        {
            return _assembly.CreateInstance(className);
        }
    }
}
