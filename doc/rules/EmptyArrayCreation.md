# EmptyArrayCreation

## Summary

Use `System.Array.Empty<T>()` instead of an empty array creation.

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
public void Method()
{
    var emptyStrings = new string[] {};
    var emptyIntegers = new int[0];
    ⋮
}
```

### Code fix

```csharp
public void Method()
{
    var emptyStrings = System.Array.Empty<string>();
    var emptyIntegers = System.Array.Empty<int>();
    ⋮
}
```
