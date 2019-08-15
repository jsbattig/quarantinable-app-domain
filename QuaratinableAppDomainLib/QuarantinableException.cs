using System;

namespace Ascentis.Framework
{
    public class QuarantinableException : Exception
    {
        public QuarantinableException(string msg) : base(msg) {}
    }
}
