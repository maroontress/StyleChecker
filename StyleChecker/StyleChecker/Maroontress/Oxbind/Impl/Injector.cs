namespace Maroontress.Oxbind.Impl
{
    /// <summary>
    /// The function to inject a value to an instance.
    /// </summary>
    /// <param name="instance">
    /// The instance to inject the value to.
    /// </param>
    /// <param name="value">
    /// The value to be injected.
    /// </param>
    public delegate void Injector(object instance, object value);
}
