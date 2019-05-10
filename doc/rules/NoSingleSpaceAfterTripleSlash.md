# NoSingleSpaceAfterTripleSlash

## Summary

Triple slash `///` (Single Line Documentation Comment) should be followed
by a single space.

## Description

This is a replacement of [SA1004][sa1004].

StyleCop.Analyzers (1.1.118) emits SA1004 to the following code:

```csharp
/// <seealso cref="LineBreakInsideAttribute
/// (string, string)"/>
/// <seealso cref="LineBreakInsideAttribute(
/// string, string)"/>
/// <seealso cref="LineBreakInsideAttribute(string,
/// string)"/>
/// <seealso cref="LineBreakInsideAttribute(string, string
/// )"/>
public void LineBreakInsideAttribute(string a, string b)
{
}
```

This analyzer does not report diagnostics to the code, which includes
a line break inside the start/end tags of an XML element,
as long as a single space follows `///`.

## Code fix

The code fix provides an option inserting a single space after `///`,
or replacing two or more spaces after `///` with a single space.
Note that Code Fix provider keeps two or more spaces in the text content
of an XML element, as well as SA1004.

## Example

### Diagnostic

```csharp
///<summary>
///  summary with extra indent.
///</summary>
///  <param name="a">first parameter.</param>
///   <param name="b">second parameter.</param>
/// <remarks>
///remarks.
/// </remarks>
public void Method(int a, int b)
{
}
```

### Code fix

```csharp
/// <summary>
///  summary with extra indent.
/// </summary>
/// <param name="a">first parameter.</param>
/// <param name="b">second parameter.</param>
/// <remarks>
/// remarks.
/// </remarks>
public void Method(int a, int b)
{
}
```

[sa1004]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1004.md
