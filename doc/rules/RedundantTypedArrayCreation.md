# RedundantTypedArrayCreation

## Summary

Use an implicitly-typed array creation instead of an explicitly-typed one.

## Description

Specifying the explicit type of the array creation is redundant if the type of
the array instance is inferred from the elements specified in the array
initializer. Note that
\[[1](#ref1)\]:

> You can create an implicitly-typed array in which the type of the array
> instance is inferred from the elements specified in the array initializer.

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
[1] [Microsoft, _Implicitly Typed Arrays (C# Reference)_][implicitly-typed-arrays-microsoft]

[implicitly-typed-arrays-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/implicitly-typed-arrays
