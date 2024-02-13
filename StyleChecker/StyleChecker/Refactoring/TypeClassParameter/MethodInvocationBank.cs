namespace StyleChecker.Refactoring.TypeClassParameter;

using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Operations;

/// <summary>
/// Represents a bank of methods and invocations, storing both the method
/// symbols and invocation operations.
/// </summary>
/// <remarks>
/// Implementation notes: because the thread that adds and the thread that gets
/// can be different, it is necessary to make them thread-safe in the memory
/// model.
/// </remarks>
public sealed class MethodInvocationBank
{
    private readonly List<IMethodSymbol> symbols = [];

    private readonly List<IInvocationOperation> operations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MethodInvocationBank"/>
    /// class.
    /// </summary>
    public MethodInvocationBank()
    {
    }

    /// <summary>
    /// Adds a collection of method symbols to the bank.
    /// </summary>
    /// <param name="all">
    /// The collection of method symbols to add.
    /// </param>
    public void AddSymbols(IEnumerable<IMethodSymbol> all)
    {
        AddRange(symbols, all);
    }

    /// <summary>
    /// Adds a collection of invocation operations to the bank.
    /// </summary>
    /// <param name="all">
    /// The collection of invocation operations to add.
    /// </param>
    public void AddOperations(IEnumerable<IInvocationOperation> all)
    {
        AddRange(operations, all);
    }

    /// <summary>
    /// Gets all method symbols in the bank.
    /// </summary>
    /// <returns>
    /// An immutable array of all method symbols in the bank.
    /// </returns>
    public ImmutableArray<IMethodSymbol> GetAllSymbols()
    {
        return ToImmutableArray(symbols);
    }

    /// <summary>
    /// Gets all invocation operations in the bank.
    /// </summary>
    /// <returns>
    /// An immutable array of all invocation operations in the bank.
    /// </returns>
    public ImmutableArray<IInvocationOperation> GetAllOperations()
    {
        return ToImmutableArray(operations);
    }

    private static void AddRange<T>(List<T> a, IEnumerable<T> all)
    {
        lock (a)
        {
            a.AddRange(all);
        }
    }

    private static ImmutableArray<T> ToImmutableArray<T>(IEnumerable<T> a)
    {
        lock (a)
        {
            return a.ToImmutableArray();
        }
    }
}
