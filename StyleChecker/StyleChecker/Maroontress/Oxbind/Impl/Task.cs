namespace Maroontress.Oxbind.Impl
{
    /// <summary>
    /// The function to dispatch values to an instance.
    /// </summary>
    /// <typeparam name="T">
    /// The type of values to dispatch to an instance.
    /// </typeparam>
    /// <param name="instance">
    /// The instance to dispatch values to.
    /// </param>
    /// <param name="value">
    /// The value to be dispatched.
    /// </param>
    public delegate void Task<T>(object instance, T value);
}
