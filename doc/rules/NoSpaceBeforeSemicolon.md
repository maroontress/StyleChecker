# NoSpaceBeforeSemicolon

## Summary

A semicolon must not be preceded by a white space.

## Description

In general, semicolons are not preceded by a space.

Note that it is intended that this analyzer and
[SpaceAfterSemicolon](../SpaceAfterSemicolon.md)
analyzer is used together, and
[SA1002](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md)
 is replaced with them.

## Code fix

The code fix provides an option eliminating spaces before the semicolon.

## Example

### Diagnostic

```csharp
public void Method()
{
    int n = 10 ;
    for (var k = 0 ; k < n ; ++k)
    {
    }
    for (; ;)
    {
    }
    for ( ;;)
    {
    }
    Console.WriteLine() /**/ ;
    return
    ;
}
```

### Code fix

```csharp
public void Method()
{
    int n = 10;
    for (var k = 0; k < n; ++k)
    {
    }
    for (;;)
    {
    }
    for (;;)
    {
    }
    Console.WriteLine() /**/;
    return;
}
```
