using System;
using System.Collections.Generic;
using System.Reflection;
using NConcern;

namespace Ascentis.Framework
{
    public class QuarantinableWrapperAttribute : Attribute, IAspect
    {
        public IEnumerable<IAdvice> Advise(MethodBase method)
        {
            yield return Advice.Basic.Around(new Func<object, object[], Func<object>, object>((_Instance, _Arguments, _Body) =>
            {
                var appDomainWrapper = (_Instance as BaseWrapperClass).LinkedAppDomainWrapper;
                if (appDomainWrapper == null)
                    return _Body();
                appDomainWrapper.TraceableMethodEnter();
                try
                {
                    var _return = _Body();
                    return _return;
                }
                catch (Exception exception)
                {
                    if (!appDomainWrapper.Parent.IsQuarantinableException(exception))
                        throw;
                    appDomainWrapper.Parent.AppDomainWrapperCompromised(appDomainWrapper);
                    throw new QuarantinableException(exception.Message);
                }
                finally
                {
                    appDomainWrapper.TraceableMethodLeave();
                }
            }));
        }
    }
}
