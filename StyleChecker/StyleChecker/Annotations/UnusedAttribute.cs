namespace StyleChecker.Annotations
{
    using System;

    /// <summary>
    /// The annotation for the UnusedVariable analyzer of StyleChecker,
    /// which marks a parameter as a unused.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Parameter,
        Inherited = false,
        AllowMultiple = false)]
    public sealed class UnusedAttribute : Attribute
    {
    }
}
