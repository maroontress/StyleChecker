<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# RedundantTypedArrayCreation

<div class="horizontal-scroll">

![RedundantTypedArrayCreation][fig-RedundantTypedArrayCreation]

</div>

## Summary

Use an implicitly-typed array creation instead of an explicitly-typed one.

## Default severity

Warning

## Description

Specifying the explicit type of the array creation is redundant if the type of
the array instance is inferred from the elements specified in the array
initializer. Note that
\[[1](#ref1)\]:

> You can create an implicitly-typed array in which the type of the array
> instance is inferred from the elements specified in the array initializer.

### Remarks

There are some cases where type inference does not work so that the
implicitly-typed arrays are not available.
For example, when all the elements are Method References, the implicitly-typed
array creation causes an error CS0826 as follows:

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

> [See errors][sharplab:examples]

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
[1] [Microsoft, _Implicitly Typed Arrays (C# Programming Guide)_][microsoft:csharp-implicitly-typed-arrays]

[microsoft:csharp-implicitly-typed-arrays]:
  https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/implicitly-typed-arrays
[sharplab:examples]:
  https://sharplab.io/#v2:C4LghgzsA0AmIGoA+ABATARgLAChcoGYACFDANhLSIFEAPMAWwAcAbAUwlwG9ci+TipCigAsRACoBPJmwCSAOwBmbAE5t5AYzkQAcgHtgAQQBuYAJYswAI3YAKAJS9+PHPzdEA9B6KGWLImzsDOrAEERgakTAABZsRMExerBEaspqmhwAdE7uRAD6RAC8RPJsAO4kGGgA2gC6Oe4uubkAInoAynoJ0WbyAObQDW4AvgDcDcO4DYSVwmJSMgpp6lqyECbmljZsDg1NzV4+wETskMd6pQFBIURmYWBEsIFsfWDAbNmuzQXFpWV1Q2cgNytlIaHsbU63V6A2B7khXTYMRhgy+uTGEymaIEsxIYgASuYIGwAMLtAAMAA40GRdtj9rkfiVygDsW4Gc0+Ajof1UZyMdjJtjpoJyHiiNykT1+nT2ZicEKgA
[fig-RedundantTypedArrayCreation]:
  https://maroontress.github.io/StyleChecker/images/RedundantTypedArrayCreation.png
