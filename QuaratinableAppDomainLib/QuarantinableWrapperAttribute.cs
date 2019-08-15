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
                (_Instance as BaseWrapperClass).LinkedAppDomain.TraceableMethodEnter();
                try
                {
                    var _return = _Body();
                    return _return;
                }
                catch (Exception exception)
                {
                    if ((_Instance as BaseWrapperClass).LinkedAppDomain.IsQuarantinableException(exception))
                        throw new QuarantinableException(exception.Message);
                    throw;
                }
                finally
                {
                    (_Instance as BaseWrapperClass).LinkedAppDomain.TraceableMethodLeave();
                }
            }));
        }
    }
}
