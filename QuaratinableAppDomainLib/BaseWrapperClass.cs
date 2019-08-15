using System;
using System.Collections.Generic;
using System.Reflection;
using NConcern;

namespace Ascentis.Framework
{
    public class BaseWrapperClass
    {
        public QuarantinableAppDomain LinkedAppDomain { get; }

        public BaseWrapperClass(QuarantinableAppDomain linkedAppDomain)
        {
            LinkedAppDomain = linkedAppDomain;
            Aspect.Weave<QuarantinableWrapperAttribute>(typeof(QuarantinableWrapperAttribute));
        }
    }
}
