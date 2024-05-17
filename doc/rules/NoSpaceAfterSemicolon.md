<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NoSpaceAfterSemicolon

<div class="horizontal-scroll">

![NoSpaceAfterSemicolon][fig-NoSpaceAfterSemicolon]

</div>

## Summary

A semicolon must be followed by a white space.

## Default severity

Warning

## Description

In most cases, a semicolon is followed by the end of
the line (EOL). If exceptionally a semicolon is followed by
other than EOL (e.g. an expression in a `for` statement,
a comment, and so on), it must be followed by a space.

However, in the style like _infinite `for` loops_,
semicolons may not be followed by a space, as follows:

```csharp
for (;;)
{
    ...
}
```

Note that this analyzer and the [SpaceBeforeSemicolon][] analyzer are intended
to be used together and replace [SA1002][] with them, allowing us to write an
infinite `for` loop with `for (;;)`.

## Code fix

The code fix provides an option inserting a space after the semicolon.

## Example

### Diagnostic

```csharp
public void Method()
{
    var n = 10;// Comment
    for (var k = 0;k < n;++k)
    {
        Console.WriteLine(k);/**/
    }
}
```

### Code fix

```csharp
public void Method()
{
    var n = 10; // Comment
    for (var k = 0; k < n; ++k)
    {
        Console.WriteLine(k); /**/
    }
}
```

[SA1002]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
[fig-NoSpaceAfterSemicolon]:
  https://maroontress.github.io/StyleChecker/images/NoSpaceAfterSemicolon.png
[SpaceBeforeSemicolon]: SpaceBeforeSemicolon.md
