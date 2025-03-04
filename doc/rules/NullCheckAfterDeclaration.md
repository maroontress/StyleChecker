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

This rule reports diagnostics when a local variable of a reference type is
declared and immediately subjected to a null check. With a declaration pattern,
you can declare a new local variable initialized with a nullable value or
reference and then check whether the value is `null`.

When the local variable declaration is an explicit type declaration and has
multiple declarators, this analyzer only covers the last variable.

This analyzer will only issue diagnostics if all of the following conditions are
true:

- the type of the declared local variable is a nullable reference type
- the expression of the initial value is nullable<sup>&dagger;</sup> (when the
  [nullable context][] is enabled)
- the local variable is unused when it may be `null`

> &dagger; If the initial value is always `null` or never `null`, the null check
> after the declaration makes no sense.

Note that the default diagnostic severity of this analyzer is
[Information][diagnostic-severity].

### How the code fix changes the meanings

Consider the following code:

```csharp
var foo = GetStringOrNull();
if (foo is null)
{
    // foo is null here
    ⋮
}
else
{
    // foo is not null here
    ⋮
}
// foo is maybe null here (*1)
⋮
```

The return value of function `GetStringOrNull()` is of `string?` type:

```cs
string? GetStringOrNull() => …;
```

The type of `foo` is `string?`, which is a nullable reference type. After the
`if` statement (*1), if `foo` is not assigned another reference in the _then_
and _else_ clauses of the `if` statement, and if `return`, `break`, `continue`,
`throw`, etc. are not in them, then `foo` after the `if` statement is _maybe_
`null` (i.e., `foo` may or may not be `null`).

Next, consider the following code that the code fix provider substituted:

```csharp
if (GetStringOrNull() is not {} foo)
{
    // foo is unassigned here
    ⋮
}
else
{
    // foo is assigned (and not null) here
    ⋮
}
// foo is unassigned here (*2)
⋮
```

The type of `foo` is `string`, which is a non-nullable reference type. After the
`if` statement (*2), `foo` is unassigned. If the state is assigned or
unassigned, it is finally unassigned.

Before the substitution, when `foo` may be `null`, using `foo` as a non-`null`
reference raises a warning like CS8604. But after the substitution, it raises an
error CS0165 (use of unassigned local variable). That is, this code fix changes
the `null` variable to the unassigned one. In other words, `null` or not `null`
changes to unassigned or not `null`. Using a local variable that may be `null`
doesn't cause an error, but using an unassigned local variable does. So this
analyzer must ignore those cases where the code fix causes errors.

> 🚩 The [IDE0019][] in Visual Studio 2022 works the same way.

For example, the following code raises a warning CS8604, so this analyzer does
not cover it:

```csharp
string? file = Environment.GetEnvironmentVariable("FILE");
if (file is null)
{
}
// The following line causes a warning CS8604
File.ReadAllText(file);
```

Adding a `throw` statement or an assignment of non-`null` reference to `file` at
the last of the _then_ clause eliminates CS8604 so that this analyzer raises the
diagnostic. It raises a diagnostic against the following code:

```csharp
string? file = Environment.GetEnvironmentVariable("FILE");
if (file is null)
{
    throw new Exception();
}
// file is not null here
File.ReadAllText(file);
```

And the code fix is also available. It also works against the following code:

```csharp
string? file = Environment.GetEnvironmentVariable("FILE");
if (file is null)
{
    file = "default.txt";
}
// file is not null here
File.ReadAllText(file);
```

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

### Special treatment of the `as` operator

This analyzer specially treats the `as` operator. In short, it imitates the
IDE0019 when the initial value is an `as` binary expression. It provides a
diagnostic for the following code:

```csharp
public void M(object o)
{
    var foo = o as string;
    if (foo is not null)
    {
        ⋮
    }
}
```

After applying the code fix, we get the following:

```csharp
public void M(object o)
{

    if (o is string foo)
    {
        ⋮
    }
}
```

## Example

### Diagnostic

```csharp
string? GetStringOrNull() => …;

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
string? GetStringOrNull() => …;

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

<a id="ref1"></a> [1] [Microsoft, _Pattern matching &mdash; the `is` and
`switch` expressions, and operators `and`, `or`, and `not` in patterns_, C#
language reference][Declaration pattern]

[diagnostic-severity]:
  https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticseverity?view=roslyn-dotnet
[logical negation operator]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/boolean-logical-operators#logical-negation-operator-
[Declaration pattern]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/patterns#declaration-and-type-patterns
[nullable context]:
  https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references#nullable-context
[IDE0019]:
  https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0019
[EqualsNull-Remarks]: EqualsNull.md#Remarks
[fig-NullCheckAfterDeclaration]:
  https://maroontress.github.io/StyleChecker/images/NullCheckAfterDeclaration.webp
