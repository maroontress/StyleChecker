# EqualsNull

## Summary

Use `is null` instead of `== null`.

## Default severity

Info

## Description

This rule reports diagnostic information of using `==` or `!=` operators
with `null` as follows:

- The operator is either `==` or `!=`.
- The right operand must be `null`.
- The type of the left operand must not be a non-nullable value type.
  (If it is a non-nullable value type, the compiler raises [CS0472][cs0472]
  and the expression is always `true` or `false`.)

Note that the default diagnostic severity of this analyzer is
[Information][diagnostic-severity].

## Code fix

The code fix provides an option replacing expression `... == null` and
`... != null` with `... is null` and `!(... is null)`, respectively.

### Remarks

It can be a breaking change to replace
the expression `... == null` with `... is null`,
as well as `... != null` with `!(... is null)`, and vice versa.
For example, the expressions `o is null` and `o == null` result in
[the same IL code](https://sharplab.io/#v2:EYLgtghgzgLgpgJwDQxASwDYB8ACAmAAgGEBYAKAG9yCaDgB7ejAgSSgDkBXDDACiIL0AlNVpiAvAD5BBNFAIA7bhgDc5UTQZMCAUQCOnCBg7L+gkWTETp9AuPGLlasgF9yQA===)
as long as its equality operators are not overridden, as follows.

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

However, when its equality operators are overridden, those expressions result in
[different IL codes](https://sharplab.io/#v2:EYLgtghgzgLgpgJwDQxASwDYB8ACAmAAgGEBYAKAG9yCaDgB7ejAgSSgDkBXDDACiIL0AlNVpiAvAD5BBNFAIA7bhgDc5UTQZMCAUQCOnCBg7L+gkWTETp9AuPGLlashoI4AzG4CMANjqNmegAHRAgYegR7AjN6LyRiQTwLK1opQWAAKzgAYxgAOgAlOAAzRDgFbLh9Q2NeWPj6JOdXD28/LUCQhDCIgEIHGLiExuSUu2le+kyc/KLShHLK6qMoOqGR5wBfciA==),
as follows.

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

Note that
the result of `o == null` may differ from the one of `o is null`
if the equality operators are *strangely* overridden as follows.

```cs
class C
{
    ...
    public static bool operator== (C o1, C o2) => true;
    public static bool operator!= (C o1, C o2) => false;
}
```

## Example

### Diagnostic

```csharp
void Method(object o, string s)
{
    if (o == null)
    {
        ...
    }
    if (s != null)
    {
        ...
    }
    ...
```

### Code fix

```csharp
void Method(object o, string s)
{
    if (o is null)
    {
        ...
    }
    if (!(s is null))
    {
        ...
    }
    ...
```

[cs0472]:
  https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0472
[diagnostic-severity]:
  https://docs.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.diagnosticseverity?view=roslyn-dotnet
