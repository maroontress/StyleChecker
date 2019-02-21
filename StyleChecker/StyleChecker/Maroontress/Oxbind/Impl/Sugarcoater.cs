namespace Maroontress.Oxbind.Impl
{
    using System.Xml;

    /// <summary>
    /// Gets an object packing the specified value.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value.
    /// </typeparam>
    /// <param name="info">
    /// The location information.
    /// </param>
    /// <param name="value">
    /// The value to be sugarcoated.
    /// </param>
    /// <returns>
    /// The value itself, or an object containing the value.
    /// </returns>
    public delegate object Sugarcoater<in T>(IXmlLineInfo info, T value);
}
