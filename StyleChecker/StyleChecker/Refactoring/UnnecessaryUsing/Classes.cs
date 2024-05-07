namespace StyleChecker.Refactoring.UnnecessaryUsing;

using System.Collections.Immutable;
using System.IO;

/// <summary>
/// Provides a set of utility methods for checking unnecessary using
/// statements.
/// </summary>
public static class Classes
{
    private static ImmutableHashSet<string> ClassNameSet { get; }
        = NewClassNameSet();

    /// <summary>
    /// Checks if the specified class disposes nothing.
    /// </summary>
    /// <param name="className">
    /// The name of the class to check.
    /// </param>
    /// <returns>
    /// <c>true</c> if the class disposes nothing; otherwise, <c>false</c>.
    /// </returns>
    public static bool DisposesNothing(string className)
        => ClassNameSet.Contains(className);

    private static ImmutableHashSet<string> NewClassNameSet() => [
            typeof(MemoryStream).FullName,
            typeof(StringReader).FullName,
            typeof(StringWriter).FullName,
            "System.IO.UnmanagedMemoryAccessor",
            "System.IO.UnmanagedMemoryStream",
        ];
}
