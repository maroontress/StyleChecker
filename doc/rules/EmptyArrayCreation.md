<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# EmptyArrayCreation

<div class="horizontal-scroll">

![EmptyArrayCreation][fig-EmptyArrayCreation]

</div>

## Summary

Use `System.Array.Empty<T>()` instead of an empty array creation.

## Default severity

Warning

## Description

An empty array is immutable, so it can be shared safely with all threads.
And creating empty arrays many times is ineffective because your CPU spends
more time for Garbage Collection. So the empty array object must be created
only once and be shared.

## Code fix

The code fix provides an option replacing the empty array creation with
`System.Array.Empty<T>()`.

## Example

### Diagnostic

```csharp
public string[] EmptyStringArray { get; }
    = new string[0];

public int[] Method()
{
    var emptyIntegers = new int[] {};
    ⋮
```

### Code fix

```csharp
public string[] EmptyStringArray { get; }
    = System.Array.Empty<string>();

public int[] Method()
{
    var emptyIntegers = System.Array.Empty<int>();
    ⋮
```

[fig-EmptyArrayCreation]:
  https://maroontress.github.io/StyleChecker/images/EmptyArrayCreation.png
