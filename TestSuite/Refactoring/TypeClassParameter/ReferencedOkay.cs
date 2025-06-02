namespace StyleChecker.Test.Refactoring.TypeClassParameter;

using System;

public sealed class ReferencedOkay
{
    private static void Log(string message)
    {
    }

    /// <summary>
    /// Print the specified type.
    /// </summary>
    /// <param name="type">
    /// The type to be printed.
    /// </param>
    public static void PrintMethod(Type type)
    {
        Log(type.FullName);
    }
}
