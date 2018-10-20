# UnnecessaryUsing

## Summary

Unnecessary using statements must be removed.

## Description

[MemoryStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream?view=netcore-2.1)
and [UnmanagedMemoryStream](https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemorystream?view=netcore-2.1)
implement [IDisposable](https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=netcore-2.1),
but dispose nothing.
See the note of them in _.NET API Browser_, which is quoted as follows: 

> Note
>
> This type implements the **IDisposable** interface, but does not actually
> have any resources to dispose. This means that disposing it by directly
> calling **Dispose()** or by using a language construct such as `using`
> (in C#) or `Using` (in Visual Basic) is not necessary.

So, creating these stream with `using` statements makes no sense.
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
