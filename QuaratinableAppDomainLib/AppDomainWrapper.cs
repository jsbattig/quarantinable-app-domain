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
        private volatile bool _compromised;
        private static volatile int _instanceCounter;
        public static int InstanceCounter => _instanceCounter;
        private volatile int _callCounter;
        public QuarantinableAppDomain Parent { get; }
        public AppDomain LinkedDomain { get; }
        public Assembly LinkedAssembly { get; }

        public void TraceableMethodEnter()
        {
            Interlocked.Increment(ref _callCounter);
        }

        public void TraceableMethodLeave()
        {
            if(Interlocked.Decrement(ref _callCounter) == 0 && _compromised)
                Unload();
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
            LinkedDomain.DomainUnload += ad_DomainUnload;
            AppDomain.Unload(LinkedDomain);
        }

        static void ad_DomainUnload(object sender, EventArgs e)
        {
        }

        public void AppDomainWrapperCompromised()
        {
            _compromised = true;
            Parent.AppDomainWrapperCompromised(this);
        }
    }
}
