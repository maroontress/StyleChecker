# UnnecessaryUsing

## Summary

Unnecessary `using` statements must be removed.

## Description

[MemoryStream][system.io.memorystream] and
[UnmanagedMemoryStream][system.io.unmanagedmemorystream]
implement [IDisposable][system.idisposable], but dispose nothing.
See the note of them\[[1](#ref1)\], which is quoted as follows:

> #### Note
>
> This type implements the `IDisposable` interface, but does not actually
> have any resources to dispose. This means that disposing it by directly
> calling `Dispose()` or by using a language construct such as `using`
> (in C#) or `Using` (in Visual Basic) is not necessary.

So, creating these stream with `using` statements doesn't make sense.
They must be created without `using` statements.

## Code fix

The code fix provides an option eliminating `using` statements.

## Example

### Diagnostic

```csharp
using System;

public class Main
{
    public void Method()
    {
        using (var s = new MemoryStream())
        {
            ...
        }
    }
}
```

### Code fix

```csharp
using System;

public class Main
{
    public void Method()
    {
        {
            var s = new MemoryStream();
            {
                ...
            }
        }
    }
}
```

## References

<a id="#ref1"></a>
[1] [Microsoft, _.NET API Browser_][dot-net-api-browser-microsoft]

[dot-net-api-browser-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/api/
[system.io.memorystream]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream?view=netcore-2.1
[system.io.unmanagedmemorystream]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemorystream?view=netcore-2.1
[system.idisposable]:
  https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=netcore-2.1
