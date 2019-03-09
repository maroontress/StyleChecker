# UnnecessaryUsing

## Summary

Unnecessary `using` statements must be removed.

## Description

[StringReader][system.io.stringreader],
[StringWriter][system.io.stringwriter],
[MemoryStream][system.io.memorystream],
[UnmanagedMemoryStream][system.io.unmanagedmemorystream] and
[UnmanagedMemoryAccessor][system.io.unmanagedmemoryaccessor]
implement [IDisposable][system.idisposable], but dispose nothing.
See the note of them \[[1](#ref1)\], which is quoted as follows:

> #### Note
>
> This type implements the `IDisposable` interface, but does not actually
> have any resources to dispose. This means that disposing it by directly
> calling `Dispose()` or by using a language construct such as `using`
> (in C#) or `Using` (in Visual Basic) is not necessary.

So, creating an instance of these classes with `using` statements doesn't
make sense. They must be created without `using` statements.

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

<a id="ref1"></a>
[1] [Microsoft, _.NET API Browser_][dot-net-api-browser-microsoft]

[dot-net-api-browser-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/api/
[system.io.memorystream]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream?view=netstandard-1.0
[system.io.unmanagedmemorystream]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemorystream?view=netstandard-2.0
[system.io.unmanagedmemoryaccessor]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemoryaccessor?view=netstandard-2.0
[system.io.stringreader]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.stringreader?view=netstandard-1.0
[system.io.stringwriter]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.stringwriter?view=netstandard-1.0
[system.idisposable]:
  https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=netstandard-1.0
