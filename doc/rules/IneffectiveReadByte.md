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
the code as follows:

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

If the underlying stream `reader.BaseStream` has no available data,
like `System.IO.MemoryStream`,
it is more simply rewritten as follows:

```csharp
var reader = new BinaryReader(...);
var buffer = new byte[1000];
var size = reader.Read(buffer, 0, 1000);
if (size < 1000)
{
    throw new EndOfStreamException();
}
```

## Code fix

The code fix is not provided.

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
