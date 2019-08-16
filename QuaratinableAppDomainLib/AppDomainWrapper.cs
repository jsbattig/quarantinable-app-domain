using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ascentis.Framework
{
    public class AppDomainWrapper
    {
        private static volatile int _instanceCounter;
        private volatile int _callCounter;
        public QuarantinableAppDomain Parent { get; }
        private AppDomain LinkedDomain { get; }
        public Assembly LinkedAssembly { get; }

        public void TraceableMethodEnter()
        {
            Interlocked.Increment(ref _callCounter);
        }

        public void TraceableMethodLeave()
        {
            Interlocked.Decrement(ref _callCounter);
        }
        
        public AppDomainWrapper(string name, QuarantinableAppDomain parent)
        {
            Parent = parent;
            var appDomainSetup = new AppDomainSetup()
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                LoaderOptimization = LoaderOptimization.MultiDomainHost
            };
            LinkedDomain = AppDomain.CreateDomain(name + Interlocked.Increment(ref _instanceCounter), AppDomain.CurrentDomain.Evidence, appDomainSetup);
            LinkedAssembly = LinkedDomain.Load(name);
        }

        public void Unload()
        {
            AppDomain.Unload(LinkedDomain);
        }
    }
}
