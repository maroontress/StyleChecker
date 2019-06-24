# IsNull

![IsNull][fig-IsNull]

## Summary

Use `== null` or `!= null` instead of `is null`.

## Default severity

Info

## Description

This rule reports diagnostic information of using `is` pattern matching
with the `null` constant literal.

Note that the default diagnostic severity of this analyzer is
[Information][diagnostic-severity].

## Code fix

The code fix provides an option replacing expression `... is null` and
`!(... is null)` with `... == null` and `... != null`, respectively.

### Remarks

Replacing the expression `... is null` with `... == null`, as well as replacing
`!(... is null)` with `... != null`, can be a breaking change.
For more information, refer to
[the description of EqualsNull code fix](EqualsNull.md#Remarks).

## Example

### Diagnostic

```csharp
public void Method(object o, string s)
{
    if (o is null)
    {
        ⋮
    }
    if (!(s is null))
    {
        ⋮
    }
    ⋮
```

### Code fix

```csharp
public void Method(object o, string s)
{
    if (o == null)
    {
        ⋮
    }
    if (s != null)
    {
        ⋮
    }
    ⋮
```

[diagnostic-severity]:
  https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticseverity?view=roslyn-dotnet
[fig-IsNull]:
  https://maroontress.github.io/StyleChecker/images/IsNull.png
