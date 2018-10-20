# IneffectiveReadByte

## Summary

Avoid invoking `System.IO.BinaryReader.ReadByte()` method in a loop.
Instead, use `Read(byte[], int, int)` method.

## Description

This analyzer reports code as follows:

```csharp
for (expr1; expr2; expr3)
{
    byteArray[i] = binaryReader.ReadByte();
}
```
where:

- `byteArray` can be any `byte[]` variable or auto-implemented property returning `byte[]`
- `binaryReader` can be any `System.IO.BinaryReader` variable
- `i` can be any `int` variable, but it must be declared in `expr1`
- `expr1` must be `int i = START` or `var i = START`
- `expr2` must be `i < END` or `i <= END`
- `expr3` must be `++i` or `i++`
- `START` and `END` are constant integers, and `START` is less than or equal to `END`

because it is ineffective and can be replaced with one invoking
`Read(byte[], int, int)`.

For example, following code invoking `ReadByte()` method in the `for` loop
is reported with the diagnostic:

```csharp
var reader = new BinaryReader(...);
var buffer = new byte[1000];
for (var i = 0; i < 1000; ++i)
{
    buffer[i] = reader.ReadByte();
}
```

The `for` loop and invoking `ReadByte()` method can be replaced with
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
in _.NET API Browser_, which are quoted as follows:

> The `Read` method will return zero only if the end of the stream is
> reached. In all other cases, `Read` always reads at least one byte from
> the stream before returning.

> An implementation is free to return fewer bytes than requested even if
> the end of the stream has not been reached.

## Code fix

The code fix provides an option replacing the `for` loop with a code
fragment, declaring an `Action` delegate and invoking it. You
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
    System.Action<byte[], int, int> _readFully = (_array, _offset, _length) =>
    {
        while (_length > 0)
        {
            var _size = reader.Read(_array, _offset, _length);
            if (_size == 0)
            {
                throw new System.IO.EndOfStreamException();
            }
            _offset += _size;
            _length -= _size;
        }
    };
    _readFully(buffer, 0, 1000);
}
```
