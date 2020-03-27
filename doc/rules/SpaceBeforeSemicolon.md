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

Note that it is intended that this analyzer and
[NoSpaceAfterSemicolon](NoSpaceAfterSemicolon.md)
analyzer are used together, and [SA1002][sa1002] is replaced with them.

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

[sa1002]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
[fig-SpaceBeforeSemicolon]:
  https://maroontress.github.io/StyleChecker/images/SpaceBeforeSemicolon.png
