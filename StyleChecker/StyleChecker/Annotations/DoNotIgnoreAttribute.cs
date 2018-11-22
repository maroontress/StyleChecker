#pragma warning disable SA1629

namespace StyleChecker.Annotations
{
    using System;

    /// <include file='docs.xml'
    /// path='docs/members[@name="DoNotIgnore"]/DoNotIgnoreAttribute/*'/>
    [AttributeUsage(
        AttributeTargets.ReturnValue,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class DoNotIgnoreAttribute : Attribute
    {
    }
}
