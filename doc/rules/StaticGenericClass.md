# StaticGenericClass

## Summary

Move type parameters from the static class to its methods if possible.

## Default severity

Warning

## Description

Replace `Class<T>.Method()` with `Class.Method<T>()` so that the data type of
the type parameter `T` can be inferred from the argument or return value.

## Code fix

The code fix provides an option moving the type parameters from the static
class to its methods.

## Example

### Diagnostic

```csharp
/// <summary>Class Summary.</summary>
/// <typeparam name="T">Type parameter.</typeparam>
public static class Code<T> where T : class
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    public static void Method(T instance)
    {
        ⋮
    }
}

public class AnotherClass
{
    public void AnotherMethod()
    {
        Code<string>.Method("...");
    }
}
```

### Code fix

```csharp
/// <summary>Class Summary.</summary>
public static class Code
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    /// <typeparam name="T">Type parameter.</typeparam>
    public static void Method<T>(T instance) where T : class
    {
        ⋮
    }
}

public class AnotherClass
{
    public void AnotherMethod()
    {
        Code.Method("...");
    }
}
```
