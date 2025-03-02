<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# RedundantTypedArrayCreation

<div class="horizontal-scroll">

![RedundantTypedArrayCreation][fig-RedundantTypedArrayCreation]

</div>

## Summary

Use an implicitly typed array creation instead of an explicitly typed one.

## Default severity

Warning

## Description

Specifying the explicit type of the array creation is redundant if the type of
the array instance can be inferred from the elements specified in the array
initializer. Note that \[[1](#ref1)\]:

> You can create an implicitly typed array in which the type of the array
> instance is inferred from the elements specified in the array initializer.

### Remarks

There are some cases where type inference does not work so that the implicitly
typed arrays are not available. For example, it doesn't work when all elements
are one of the following:

- Target-typed `new` expression \[[2](#ref2)\] (`new(…)`)
- Collection expression (`[…]`)
- `null` literal
- `default` literal

There are also some cases where the implicitly typed arrays change the meaning.
For example, the element of `array` is of type `int?` (`Nullable<int>`) in the
following code:

```cs
var array = new int?[] { 42, };
```

Let's leave the array initializer and remove `int?` from the code. Then, the
element of `array` is of type `int`, as follows:

```cs
var array = new[] { 42, };
```

### C# 9 and earlier versions

Before C# 10, the implicitly typed array creation causes an error CS0826 when
all elements are Method References as follows:

```csharp
public static void RaiseCS0826()
{
    _ = new[]
    {
        DoSomething,
    };
}

public static void DoSomething()
{
}
```

> [See errors in C# 9][sharplab:examples]

This analyzer ignores the explicitly typed arrays where all the elements are
Method References in C# 9 and earlier.

### Alternatives

In some cases, it is a good idea to create explicitly typed arrays. For example:

```cs
var array = new[]
{
    new Foo(1),
    new Foo(2),
};
```

If it is desirable to describe `Foo` less often, you can refactor this code as
follows:

```cs
var array = new Foo[]
{
    new(1),
    new(2),
};
```

In most cases, collection expressions are preferable to implicitly typed array
creations:

```cs
// Collection expression
public IEnumerable<string> GetStrings() => ["foo", "bar"];
```

```cs
// Implicitly typed array creation
public IEnumerable<string> GetStrings() => new[] {"foo", "bar"};
```

## Code fix

The code fix provides an option removing the explicit type of the array.

## Example

### Diagnostic

```csharp
public void Method()
{
    var all = new string[] { "a", "b", "c", };
    ⋮
}
```

### Code fix

```csharp
public void Method()
{
    var all = new[] { "a", "b", "c", };
    ⋮
}
```

## References

<a id="ref1"></a>
[1] [Microsoft, _Implicitly Typed Arrays_, C# language reference][microsoft:csharp-implicitly-typed-arrays]

<a id="ref2"></a>
[2] [Microsoft, _Target-typed `new` expressions_, C# language reference][microsoft:csharp-target-typed-new]

[microsoft:csharp-implicitly-typed-arrays]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays#implicitly-typed-arrays
[microsoft:csharp-target-typed-new]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-9.0/target-typed-new
[sharplab:examples]:
  https://sharplab.io/#v2:C4LghgzsA0AmIGoA+ABATARgLAChcoGYACFDANhLSIFEAPMAWwAcAbAUwlwG9ci+TipCigAsRACoBPJmwCSAOwBmbAE5t5AYzkQAcgHtgAQQBuYAJYswAI3YAKAJS9+PHPzdEA9B6KGWLImzsDOrAEERgakTAABZsRMExerBEaspqmhwAdE7uRAD6RAC8RPJsAO4kGGgA2gC6Oe4uubkAInoAynoJ0WbyAObQDW4AvgDcDcO4DYSVwmJSMgpp6lqyECbmljZsDg1NzV4+wETskMd6pQFBIURmYWBEsIFsfWDAbNmuzQXFpWV1Q2cgNytlIaHsbU63V6A2B7khXTYMRhgy+uTGEymaIEsxIYgASuYIGwAMLtAAMAA40GRdtj9rkfiVygDsW4Gc0+Ajof1UZyMdjJtjpoJyHiiNykT1+nT2ZicEKgA
[fig-RedundantTypedArrayCreation]:
  https://maroontress.github.io/StyleChecker/images/RedundantTypedArrayCreation.png
