<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# EqualsNull

<div class="horizontal-scroll">

![EqualsNull][fig-EqualsNull]

</div>

## Summary

Use &ldquo;`… is null`&rdquo; or &ldquo;`… is {}`&rdquo; instead of
&ldquo;`… == null`.&rdquo;

## Default severity

Info

## Description

This rule reports diagnostic information of using the equality operators
(&ldquo;`==`&rdquo; and &ldquo;`!=`&rdquo;) with `null` as follows:

- The operator is either &ldquo;`==`&rdquo; or &ldquo;`!=`.&rdquo;
- The right operand is `null`.
- The type of the left operand is not a non-nullable value type.
  (If it is a non-nullable value type, the compiler raises [CS0472][cs0472]
  and the expression is always `true` or `false`.)

Note that the default diagnostic severity of this analyzer is
[Information][diagnostic-severity].

## Code fix

The code fix provides an option replacing the expressions
&ldquo;`… == null`&rdquo; and &ldquo;`… != null`&rdquo; with
&ldquo;`… is null`&rdquo; and &ldquo;`!(… is null)`,&rdquo; respectively.
Also, it provides another option replacing them with &ldquo;`!(… is {})`&rdquo;
and &ldquo;`… is {}`,&rdquo; respectively.

### Remarks

It can be a breaking change to replace the expression &ldquo;`… == null`&rdquo;
with &ldquo;`… is null`,&rdquo; as well as &ldquo;`… != null`&rdquo; with
&ldquo;`!(… is null)`,&rdquo; and vice versa.  For example, the expressions
&ldquo;`o is null`&rdquo; and &ldquo;`o == null`&rdquo; result in
[the same IL code][example-same-il-codes] as long as its equality operators
are not overridden, as follows:

```cs
class C
{
    bool IsNull(C o) => o is null;
    /*
        IL_0000: ldarg.1
        IL_0001: ldnull
        IL_0002: ceq
        IL_0004: ret
    */

    bool EqualsNull(C o) => o == null;
    /*
        IL_0000: ldarg.1
        IL_0001: ldnull
        IL_0002: ceq
        IL_0004: ret
    */
}
```

However, when its equality operators are overridden, those expressions result
in [different IL codes][example-different-il-codes], as follows:

```cs
class C
{
    bool IsNull(C o) => o is null;
    /*
        IL_0000: ldarg.1
        IL_0001: ldnull
        IL_0002: ceq
        IL_0004: ret
    */

    bool EqualsNull(C o) => o == null;
    /*
        IL_0000: ldarg.1
        IL_0001: ldnull
        IL_0002: call bool C::op_Equality(class C, class C)
        IL_0007: ret
    */

    public static bool operator== (C o1, C o2)
        => object.ReferenceEquals(o1, o2);

    public static bool operator!= (C o1, C o2)
        => !object.ReferenceEquals(o1, o2);
}
```

Note that the result of &ldquo;`o == null`&rdquo; may differ from the one of
&ldquo;`o is null`&rdquo; if the equality operators are *strangely* overridden
as follows:

```cs
class C
{
    ⋮
    public static bool operator== (C o1, C o2) => true;
    public static bool operator!= (C o1, C o2) => false;
}
```

## Example

### Diagnostic

```csharp
public void Method(object? o, string? s)
{
    if (o == null && s != null)
    {
        ⋮
    }
    ⋮
```

### Code fix (with the constant pattern)

```csharp
public void Method(object? o, string? s)
{
    if (o is null && !(s is null))
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

[cs0472]:
  https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0472
[diagnostic-severity]:
  https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticseverity?view=roslyn-dotnet
[fig-EqualsNull]:
  https://maroontress.github.io/StyleChecker/images/EqualsNull.png
[example-same-il-codes]:
  https://sharplab.io/#v2:EYLgtghgzgLgpgJwDQxASwDYB8ACAmAAgGEBYAKAG9yCaDgB7ejAgSSgDkBXDDACiIL0AlNVpiAvAD5BBNFAIA7bhgDc5UTQZMCAUQCOnCBg7L+gkWTETp9AuPGLlasgF9yQA===
[example-different-il-codes]:
  https://sharplab.io/#v2:EYLgtghgzgLgpgJwDQxASwDYB8ACAmAAgGEBYAKAG9yCaDgB7ejAgSSgDkBXDDACiIL0AlNVpiAvAD5BBNFAIA7bhgDc5UTQZMCAUQCOnCBg7L+gkWTETp9AuPGLlashoI4AzG4CMANjqNmegAHRAgYegR7AjN6LyRiQTwLK1opQWAAKzgAYxgAOgAlOAAzRDgFbLh9Q2NeWPj6JOdXD28/LUCQhDCIgEIHGLiExuSUu2le+kyc/KLShHLK6qMoOqGR5wBfciA==
