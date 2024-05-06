<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# IsNull

<div class="horizontal-scroll">

![IsNull][fig-IsNull]

</div>

## Summary

Use &ldquo;`… == null`&rdquo; or &ldquo;`… is {}`&rdquo; instead of
&ldquo;`… is null`.&rdquo;

## Default severity

Info

## Description

This rule reports diagnostic information of using `is` pattern matching with
the `null` Constant Pattern.

Note that the default diagnostic severity of this analyzer is
[Information][diagnostic-severity].

## Code fix

The code fix provides an option replacing the expressions
&ldquo;`… is null`&rdquo; and &ldquo;`!(… is null)`&rdquo; with
&ldquo;`… == null`&rdquo; and &ldquo;`… != null`,&rdquo; respectively.  Also,
it provides another option replacing them with &ldquo;`!(… is {})`&rdquo; and
&ldquo;`… is {}`,&rdquo; respectively.

### Remarks

Replacing the expression &ldquo;`… is null`&rdquo; with
&ldquo;`… == null`,&rdquo; as well as replacing &ldquo;`!(… is null)`&rdquo;
with &ldquo;`… != null`,&rdquo; can be a breaking change.  For more
information, refer to
[the description of EqualsNull code fix](EqualsNull.md#Remarks).

## Example

### Diagnostic

```csharp
public void Method(object? o, string? s)
{
    if (o is null && !(s is null))
    {
        ⋮
    }
    ⋮
```

### Code fix (with the equality operators)

```csharp
public void Method(object? o, string? s)
{
    if (o == null && s != null)
    {
        ⋮
    }
    ⋮
```

### Code fix (with the property pattern)

```csharp
public void Method(object? o, string? s)
{
    if (!(o is {}) && s is {})
    {
        ⋮
    }
    ⋮
```

[diagnostic-severity]:
  https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticseverity?view=roslyn-dotnet
[fig-IsNull]:
  https://maroontress.github.io/StyleChecker/images/IsNull.png
