<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# IneffectiveReadByte

<div class="horizontal-scroll">

![IneffectiveReadByte][fig-IneffectiveReadByte]

</div>

## Summary

Avoid invoking `System.IO.BinaryReader.ReadByte()` method in a loop.
Instead, use `Read(byte[], int, int)` method.

## Default severity

Warning

## Description

This analyzer reports diagnostics for the following code:

```csharp
for (expr1; expr2; expr3)
{
    byteArray[i] = binaryReader.ReadByte();
}
```

where:

- `byteArray` can be any `byte[]` variable or auto-implemented property
  returning `byte[]`
- `binaryReader` can be any `System.IO.BinaryReader` variable or
  auto-implemented property returning `System.IO.BinaryReader`
- `i` can be any `int` variable, but it must be declared in `expr1`
- `expr1` must be `int i = START` or `var i = START`
- `expr2` must be `i < END` or `i <= END`
- `expr3` must be `++i` or `i++`
- `START` and `END` are constant integers, and `START` is less than or equal
  to `END`

because it is ineffective and can be replaced with more effective one invoking
`Read(byte[], int, int)`.

For example, following code invoking `ReadByte()` method in the `for` loop
is reported with the diagnostic:

```csharp
BinaryReader reader = ...;
byte[] buffer = ...;

for (var i = 0; i < 1000; ++i)
{
    buffer[i] = reader.ReadByte();
}
```

The `for` loop and invoking `ReadByte()` method can be replaced with
the `readFully`-like code as follows:

```csharp
BinaryReader reader = ...;
byte[] buffer = ...;

var offset = 0;
var length = 1000;
while (length > 0)
{
    var size = reader.Read(buffer, offset, length);
    if (size is 0)
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
BinaryReader reader = ...;
byte[] buffer = ...;

var size = reader.Read(buffer, 0, 1000);
if (size < 1000)
{
    throw new EndOfStreamException();
}
```

However, even `System.IO.MemoryStream` doesn't guarantee
to read requested bytes when the end of the stream has not been reached.
See the specifications of
[MemoryStream.Read Method][system.io.memorystream.read]
\[[1](#ref1)\], which are quoted as follows:

> The `Read` method will return zero only if the end of the stream is
> reached. In all other cases, `Read` always reads at least one byte from
> the stream before returning.
>
> &vellip;
>
> An implementation is free to return fewer bytes than requested even if
> the end of the stream has not been reached.

## Code fix

The code fix provides an option replacing the `for` loop with a code
fragment, declaring an `Action` delegate and invoking it. You
should refactor the auto-generated code with renaming identifiers and
replacing the delegate with the local function or extension method
if possible.

## Example

### Diagnostic

```csharp
public void Method(Stream inputStream)
{
    var reader = new BinaryReader(inputStream);
    var buffer = new byte[1000];

    for (var i = 0; i < 1000; ++i)
    {
        buffer[i] = reader.ReadByte();
    }
}
```

### Code fix

```csharp
public void Method(Stream inputStream)
{
    var reader = new BinaryReader(inputStream);
    var buffer = new byte[1000];

    {
        System.Action<byte[], int, int> _readFully = (_array, _offset, _length) =>
        {
            var _reader = reader;
            while (_length > 0)
            {
                var _size = _reader.Read(_array, _offset, _length);
                if (_size is 0)
                {
                    throw new System.IO.EndOfStreamException();
                }
                _offset += _size;
                _length -= _size;
            }
        };
        _readFully(buffer, 0, 1000);
    }
}
```

## References

<a id="ref1"></a>
[1] [Microsoft, _.NET API Browser_][dot-net-api-browser-microsoft]

[dot-net-api-browser-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/api/
[system.io.memorystream.read]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream.read?view=netcore-2.1#System_IO_MemoryStream_Read_System_Byte___System_Int32_System_Int32_
[fig-IneffectiveReadByte]:
  https://maroontress.github.io/StyleChecker/images/IneffectiveReadByte.png
