# DiscardingReturnValue

## Summary

Do not discard the return value of some methods.

## Description

There are delicate methods that return a regardful value as follows:

- `System.IO.Stream.Read(byte[], int, int)`
- `System.IO.BinaryReader.Read(byte[], int, int)`

### Read &mdash; POSIX read(2) style methods

These `read` methods don't guarantee to read requested bytes
even when the end of the stream has not been reached.
See the specifications of
[Stream.Read Method][system.io.stream.read]
\[[1](#ref1)\], which are quoted as follows:

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
\[[1](#ref1)\], which are quoted as follows:

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
}
```

## References

<a id="ref1"></a>
[1] [Microsoft, _.NET API Browser_][dot-net-api-browser-microsoft]

[dot-net-api-browser-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/api/
[system.io.stream.read]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.stream.read?view=netcore-2.1#System_IO_Stream_Read_System_Byte___System_Int32_System_Int32_
[system.io.binaryreader.read]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.binaryreader.read?view=netcore-2.1#System_IO_BinaryReader_Read_System_Byte___System_Int32_System_Int32_
