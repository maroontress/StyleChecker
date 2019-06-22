# DiscardingReturnValue

## Summary

Do not discard the return value of some methods.

## Default severity

Warning

## Description

There are delicate methods that return a regardful value,
or that do not make sense if the return value is discarded.
This rule reports diagnostic information as a warning similar to
[CA1806 (Do not ignore method results)][ca1806]\[[1](#ref1)\]
about discarding the return value of the methods as follows:

- Some `Read` methods returning the number of bytes read actually
- Some methods of immutable classes (e.g. `string`, `ImmutableArray`, ...)
- Methods whose return value is annotated with the custom attribute
- Methods specified with the configuration file `StyleChecker.xml`

## Read &mdash; POSIX read(2) style methods

The following methods are covered:

- `System.IO.Stream.Read(byte[], int, int)`
- `System.IO.BinaryReader.Read(byte[], int, int)`

These `read` methods don't guarantee to read requested bytes
even when the end of the stream has not been reached.
See the specifications of
[Stream.Read Method][system.io.stream.read]
\[[2](#ref2)\], which are quoted as follows:

> ### Remarks
>
> ... Implementations return the number of bytes read. The implementation will
> block until at least one byte of data can be read, in the event that no data
> is available. Read returns 0 only when there is no more data in the stream
> and no more is expected (such as a closed socket or end of file). An
> implementation is free to return fewer bytes than requested even if the end
> of the stream has not been reached.

And the specifications of
[BinaryReader.Read Method][system.io.binaryreader.read]
\[[2](#ref2)\], which are quoted as follows:

> ### Returns
>
> The number of bytes read into buffer. This might be less than the number
> of bytes requested if that many bytes are not available, or it might be zero
> if the end of the stream is reached.

So, if the return value is discarded, the actual length of read bytes is
unknown, which doesn't make sense.

There is a useful common pattern using the `readFully`-like code as follows:

```csharp
Stream stream = ...;
byte[] buffer = ...;
int initialOffset = ...;
int offset = initialOffset
int length = ...;

while (length > 0)
{
    var size = stream.Read(buffer, offset, length);
    if (size == 0)
    {
        break;
        // or throw new EndOfStreamException();
    }
    offset += size;
    length -= size;
}

// Here, the actual read length is (offset - initialOffset).
```

## Methods of immutable types

The following methods, that have no side effects
and that do not make sense if the return value is discarded, are
subject to the diagnostics:

- all `string` methods (except ones returning `void`)
- all `System.Type` methods (except ones returning `void`
  and `InvokeMember` methods)
- all methods of
  `ImmutableArray`,
  `ImmutableDictionary`,
  `ImmutableHashSet`,
  `ImmutableList`,
  `ImmutableQueue`,
  `ImmutableSortedDictionary`,
  `ImmutableSortedSet`,
  `ImmutableStack`
  types in namespace `System.Collections.Immutable`

The description of [CA1806][ca1806] is quoted as follows:

> Rule Description
>
> Unnecessary object creation and the associated garbage collection of
> the unused object degrade performance.

However, those who discard the return value of the method having no side effects
are just confused in many cases. For example, the `string` modification methods
are typical. The specifications of
[System.String Class][system.string.modifying-string]
\[[2](#ref2)\]
are quoted as follows:

> **Important**
>
> All string modification methods return a new String object. They don't modify
> the value of the current instance.

It is important that all modification methods of immutable objects
always return the new _unshared_ object for every call,
so discarding the return value of those methods doesn't make sense.
In the same way, any other methods without side effects also are wasteful
 if their return value is ignored.

## Methods whose return value is annotated

The methods that are not of the standard API can be covered
with `DoNotIgnoreAttribute` provided with
[StyleChecker.Annotations][stylechecker-annotations].
The methods are covered if the return value is annotated
with `DoNotIgnoreAttribute` as follows:

```csharp
using Maroontress.StyleChecker.Annotations;

public class Class
{
    [return: DoNotIgnore]
    public void Method()
    {
        return new ImmutableValue();
    }
}
```

## Methods specified with the configuration file

If `DoNotIgnoreAttribute` is not available,
you can specify methods with the configuration file `StyleChecker.xml`.
For example, if you would like to make sure that the return values of
`int.Parse(string)` method and `Array.Empty<T>()` method are not discarded,
edit `StyleChecker.xml` file as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<config xmlns="https://maroontress.com/StyleChecker/config.v1">
  ⋮
  <DiscardingReturnValue>
    <method id="int.Parse(string)"/>
    <method id="System.Array.Empty&lt;T&gt;()"/>
  </DiscardingReturnValue>
  ⋮
</config>
```

The `DiscardingReturnValue` element can have `method` elements
as its child elements,
and the `id` attribute of the `method` element specifies the method whose
return value must not be ignored. The value of `id` attribute
must represent the fully-qualified type name (of the type containing it)
followed by its name and parameter types as follows:

```
FullyQualifiedTypeName.MethodName(ParameterTypeName1, ParameterTypeName2, ...)
```

The type name must be fully-qualified,
but if there is the keyword for the type,
it must be the keyword instead of the type name.
For example, use `int` instead of `System.Int32`,
`string` instead of `System.String`, and so on.

If the type or method is a generic one,
the name must be of the original definition.
Note that the symbols '`<`' and '`>`' have to be escaped in an XML document
with "`&lt;`" and "`&gt;`", respectively.

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
public void Method()
{
    BinaryReader reader = ...;
    byte[] buffer = ...;

    reader.Read(buffer, 0, buffer.Length);

    "hello".IndexOf("o");
}
```

## References

<a id="ref1"></a>
[1] [Microsoft, _Code Analysis for Managed Code Warnings_][ca-warnings-microsoft]

<a id="ref2"></a>
[2] [Microsoft, _.NET API Browser_][dot-net-api-browser-microsoft]

[ca1806]:
  https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1806-do-not-ignore-method-results?view=vs-2017
[dot-net-api-browser-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/api/
[ca-warnings-microsoft]:
  https://docs.microsoft.com/en-us/visualstudio/code-quality/code-analysis-for-managed-code-warnings?view=vs-2017
[system.io.stream.read]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.read?view=netcore-2.1#System_IO_Stream_Read_System_Byte___System_Int32_System_Int32_
[system.io.binaryreader.read]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.binaryreader.read?view=netcore-2.1#System_IO_BinaryReader_Read_System_Byte___System_Int32_System_Int32_
[system.string.modifying-string]:
  https://docs.microsoft.com/en-us//dotnet/api/system.string?view=netframework-4.7.2#modifying-a-string
[stylechecker-annotations]:
  https://www.nuget.org/packages/StyleChecker.Annotations/
