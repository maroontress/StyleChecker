namespace StyleChecker;

using System;
using System.Resources;
using Microsoft.CodeAnalysis;

/// <summary>
/// Provides a string localizer.
/// </summary>
public static class Localizers
{
    /// <summary>
    /// Returns a new function that takes a <c>string</c> parameter and
    /// returns a <c>LocalizableString</c> associated with the parameter.
    /// </summary>
    /// <param name="manager">
    /// The <c>ResourceManager</c> object.
    /// </param>
    /// <typeparam name="T">
    /// The type of the auto-generated <c>Resources</c> class.
    /// </typeparam>
    /// <returns>
    /// The new function mapping from a key <c>string</c> to a
    /// <c>LocalizableString</c>.
    /// </returns>
    public static Func<string, LocalizableString>
            Of<T>(ResourceManager manager)
        => s => new LocalizableResourceString(s, manager, typeof(T));
}
