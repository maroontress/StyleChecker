namespace StyleChecker.Analyzers.Refactoring.TypeClassParameter;

using System.Collections.Frozen;
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
    /// <summary>
    /// Initializes a new instance of the <see cref="MethodInvocationBank"/>
    /// class.
    /// </summary>
    public MethodInvocationBank()
    {
    }

    private List<IMethodSymbol> Symbols { get; } = [];

    private List<IInvocationOperation> Operations { get; } = [];

    private HashSet<IMethodSymbol> ReferenceSymbolSet { get; } = [];

    /// <summary>
    /// Adds a collection of method symbols to the bank.
    /// </summary>
    /// <param name="all">
    /// The collection of method symbols to add.
    /// </param>
    public void AddSymbols(IEnumerable<IMethodSymbol> all)
    {
        AddRange(Symbols, all);
    }

    /// <summary>
    /// Adds a collection of invocation operations to the bank.
    /// </summary>
    /// <param name="all">
    /// The collection of invocation operations to add.
    /// </param>
    public void AddOperations(IEnumerable<IInvocationOperation> all)
    {
        AddRange(Operations, all);
    }

    /// <summary>
    /// Adds a collection of Method Reference symbols to the bank.
    /// </summary>
    /// <param name="all">
    /// The collection of method reference symbols to add.
    /// </param>
    public void AddReferenceSymbols(IEnumerable<IMethodSymbol> all)
    {
        lock (ReferenceSymbolSet)
        {
            ReferenceSymbolSet.UnionWith(all);
        }
    }

    /// <summary>
    /// Gets all method symbols in the bank.
    /// </summary>
    /// <returns>
    /// An immutable array of all method symbols in the bank.
    /// </returns>
    public FrozenSet<IMethodSymbol> GetAllSymbols()
    {
        return ToFrozenSet(Symbols);
    }

    /// <summary>
    /// Gets all invocation operations in the bank.
    /// </summary>
    /// <returns>
    /// An immutable array of all invocation operations in the bank.
    /// </returns>
    public ImmutableArray<IInvocationOperation> GetAllOperations()
    {
        return ToImmutableArray(Operations);
    }

    /// <summary>
    /// Gets all Method Reference symbols in the bank.
    /// </summary>
    /// <returns>
    /// A frozen set of all Method Reference symbols in the bank.
    /// </returns>
    public FrozenSet<IMethodSymbol> GetAllReferenceSymbols()
    {
        return ToFrozenSet(ReferenceSymbolSet);
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
            return [.. a];
        }
    }

    private static FrozenSet<T> ToFrozenSet<T>(IEnumerable<T> a)
    {
        lock (a)
        {
            return a.ToFrozenSet();
        }
    }
}
