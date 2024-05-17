<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# SpaceBeforeSemicolon

<div class="horizontal-scroll">

![SpaceBeforeSemicolon][fig-SpaceBeforeSemicolon]

</div>

## Summary

A semicolon must not be preceded by a white space.

## Default severity

Warning

## Description

In general, semicolons are not preceded by a space.

Note that this analyzer and the [NoSpaceAfterSemicolon][] analyzer are intended
to be used together and replace [SA1002][] with them, allowing us to write an
infinite `for` loop with `for (;;)`.

## Code fix

The code fix provides an option eliminating spaces before the semicolon.

## Example

### Diagnostic

```csharp
public void Method()
{
    var n = 10 ;
    Console.WriteLine() /**/ ;
    for (var k = 0 ; k < n ; ++k)
    {
    }
    for ( ; ;)
    {
        return
        ;
    }
}
```

### Code fix

```csharp
public void Method()
{
    var n = 10;
    Console.WriteLine() /**/;
    for (var k = 0; k < n; ++k)
    {
    }
    for (;;)
    {
        return;
    }
}
```

[SA1002]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
[fig-SpaceBeforeSemicolon]:
  https://maroontress.github.io/StyleChecker/images/SpaceBeforeSemicolon.png
[NoSpaceAfterSemicolon]: NoSpaceAfterSemicolon.md
