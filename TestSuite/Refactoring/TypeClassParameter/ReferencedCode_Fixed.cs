#pragma warning disable CS8019

namespace StyleChecker.Test.Refactoring.TypeClassParameter;

using System;

public sealed class ReferencedCode
{
    private static void Log(string message)
    {
    }

    /// <summary>
    /// Print the specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type to be printed.
    /// </typeparam>
    public static void PrintMethod<T>()
    {
        var type = typeof(T);
        Log(type.FullName);
    }

    /// <summary>
    /// Print the specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type to be ignored.
    /// </typeparam>
    /// <typeparam name="T0">
    /// The type to be printed.
    /// </typeparam>
    public void Print<T, T0>()
    {
        var type = typeof(T0);
        Log(type.FullName);
    }
}
