<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NullCheckAfterDeclaration

<div class="horizontal-scroll">

![NullCheckAfterDeclaration][fig-NullCheckAfterDeclaration]

</div>

## Summary

Do not declare a local variable and then check to see whether it is null:

```csharp
var foo = …;
if (foo is null)
{
    ⋮
}
```

Instead, use pattern matching (with a declaration pattern \[[1](#ref1)\]) as
follows:

```csharp
if (… is not {} foo)
{
    ⋮
}
```

## Default severity

Info

## Description

This rule reports diagnostic information of the declaration of the local
variable followed by the null check if the initial value is a reference. With a
declaration pattern, you can declare a new local variable initialized with a
nullable value or reference and then check whether the value is `null`.

When the local variable declaration is an explicit type declaration and has
multiple declarators, this analyzer covers only the last variable.

This analyzer does not cover cases where the declared local variables are of
value types.

Note that the default diagnostic severity of this analyzer is
[Information][diagnostic-severity].

## Code fix

The code fix provides an option to replace the declaration of the local variable
followed by the null check with the corresponding declaration pattern.

There are combinations of implicit and explicit types for declarations, and
`null` and not `null` for conditions. The following two tables show the
relationship between those combinations and code fixes.

| Declaration | Condition | Code Fix |
| :---: | :---: | :---: |
| `var foo = …;` | NotNull | `if (… is {} foo)`|
| `var foo = …;` | Null | `if (… is {} not foo)`|
| `Bar? foo = …;` | NotNull | `if (… is Bar foo)`|
| `Bar? foo = …;` | Null | `if (… is not Bar foo)`|

| Condition | Code |
| :---: | :---: |
| NotNull | `(foo is not null)`, `(foo is {})`, or `(foo != null)` |
| Null | `(foo is null)`, `(foo is not {})`, or `(foo == null)` |

Note that `foo` represents the name of a local variable, and `Bar` represents
the type name. Conditions containing the [logical negation operator][] (`!`) are
not covered (e.g., `(!(foo == null))`, `(!(foo is null))`).

### Remarks

It can be a breaking change to replace the expression `… == null` or `… != null`
when these operators are overridden. For more information, refer to the
[description of EqualsNull code fix][EqualsNull-Remarks].

## Example

### Diagnostic

```csharp
string? GetStringOrNull()
{
    ⋮
}

var foo = GetStringOrNull();
if (foo is null)
{
    ⋮
}

string? bar = GetStringOrNull();
if (bar is null)
{
    ⋮
}
```

### Code fix

```csharp
string? GetStringOrNull()
{
    ⋮
}

if (GetStringOrNull() is not
    {
    } foo)
{
    ⋮
}

if (GetStringOrNull() is not string bar)
{
    ⋮
}
```

## References

<a id="ref1"></a> [1]
[Microsoft, _Pattern matching - the is and switch expressions, and operators and, or, and not in patterns_][Declaration pattern]

[diagnostic-severity]:
  https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticseverity?view=roslyn-dotnet
[logical negation operator]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators#logical-negation-operator-
[Declaration pattern]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#declaration-and-type-patterns
[EqualsNull-Remarks]: EqualsNull.md#Remarks
[fig-NullCheckAfterDeclaration]:
  https://maroontress.github.io/StyleChecker/images/NullCheckAfterDeclaration.webp
