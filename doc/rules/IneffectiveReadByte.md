# IneffectiveReadByte

## Summary

Avoid invocating `System.IO.BinaryReader.ReadByte()` method within a loop.
Instead, use `Read(byte[], int, int)` method.

## Description

Following code using `ReadByte()` is ineffective.

```csharp
var reader = new BinaryReader(...);
var buffer = new byte[1000];
for (var i = 0; i < 1000; ++i)
{
    buffer[i] = reader.ReadByte();
}
```

The `for` loop and invocating `ReadByte()` method can be replaced with
the `readFully`-like code as follows:

```csharp
var reader = new BinaryReader(...);
var buffer = new byte[1000];
var offset = 0;
var length = 1000;
while (length > 0)
{
    var size = reader.Read(buffer, offset, length);
    if (size == 0)
    {
        throw new EndOfStreamException();
    }
    offset += size;
    length -= size;
}
```

If the underlying stream `reader.BaseStream` has always available data
except for end of stream, it is more simply rewritten as follows:

```csharp
var reader = new BinaryReader(...);
var buffer = new byte[1000];
var size = reader.Read(buffer, 0, 1000);
if (size < 1000)
{
    throw new EndOfStreamException();
}
```

However, even `System.IO.MemoryStream` doesn't guarantee
to read requested bytes when the end of the stream has not been reached.
See the specifications of
**[MemoryStream.Read Method](https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream.read?view=netcore-2.1#System_IO_MemoryStream_Read_System_Byte___System_Int32_System_Int32_)**
in _.NET API Browser_, which is quoted as follows:

> The `Read` method will return zero only if the end of the stream is
> reached. In all other cases, `Read` always reads at least one byte from
> the stream before returning.

> An implementation is free to return fewer bytes than requested even if
> the end of the stream has not been reached.

## Code fix

The code fix provides an option replacing the `for` loop with a code
fragment, declaring an `Action` delegate and invocating it. You
should refactor the auto-generated code with renaming identifers and
replacing the delegate with the local function or extension method
if possible.

## Example

### Diagnostic

```csharp
public void Method()
{
    var reader = new BinaryReader(...);
    var buffer = new byte[1000];
    for (var i = 0; i < 1000; ++i)
    {
        buffer[i] = reader.ReadByte();
    }
}
```

### Code fix

```csharp
public void Method()
{
    var reader = new BinaryReader(...);
    var buffer = new byte[1000];
    System.Action<int, int> _readFully = (_offset, _length) =>
    {
        while (_length > 0)
        {
            var _size = reader.Read(buffer, _offset, _length);
            if (_size == 0)
            {
                throw new System.IO.EndOfStreamException();
            }
            _offset += _size;
            _length -= _size;
        }
    };
    _readFully(0, 1000);
}
```
