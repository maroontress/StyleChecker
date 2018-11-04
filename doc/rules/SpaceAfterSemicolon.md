# SpaceAfterSemicolon

## Summary

A semicolon must be followed by a white space.

## Description

In most cases, semicolons are followed by the end of
the line (EOL). If exceptionally semicolons is followed by
other than EOL (e.g. an expression in a `for` statement,
a comment, and so on), they must be followed by a space.

However, in the style like infinite `for` loops as follows,
semicolons may not be followed by a space.

```csharp
for (;;)
{
    ...
}
```

Note that it is intended that this analyzer and
[NoSpaceBeforeSemicolon](NoSpaceBeforeSemicolon.md)
analyzer is used together, and [SA1002][sa1002] is replaced with them.

## Code fix

The code fix provides an option inserting a space after the semicolon.

## Example

### Diagnostic

```csharp
public void Method()
{
    int n = 10;// Comment
    for (var k = 0;k < n;++k)
    {
    }
}
```

### Code fix

```csharp
public void Method()
{
    int n = 10; // Comment
    for (var k = 0; k < n; ++k)
    {
    }
}
```

[sa1002]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
