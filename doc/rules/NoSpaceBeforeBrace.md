<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NoSpaceBeforeBrace

<div class="horizontal-scroll">

![NoSpaceBeforeBrace][fig-NoSpaceBeforeBrace]

</div>

## Summary

A brace (&lsquo;`{`&rsquo; or &lsquo;`}`&rsquo;) must be preceded by a white
space.

## Default severity

Warning

## Description

An opening brace (&lsquo;`{`&rsquo;) that is not the first character on the
line must be preceded by a single space or an opening parenthesis
(&lsquo;`(`&rsquo;).

A closing brace (&lsquo;`}`&rsquo;) that is not the first character on the line
must be preceded by a single space or an opening brace (&lsquo;`{`&rsquo;).

Note that this analyzer and [NoSpaceAfterBrace](NoSpaceAfterBrace.md) analyzer
are designed to use them together and replace [SA1012][sa1012] and
[SA1013][sa1013] with them, allowing us to write empty braces
(&ldquo;`{}`&rdquo;) as follows:

```csharp
Action doNothing = () => {};

if (maybeString is {} s)
{
    ⋮
}
```

## Code fix

The code fix provides an option inserting a space before the brace.

## Example

### Diagnostic

```csharp
string[] array ={ ""};

Action doNothing = () =>{ return;};

if (array is{ Length: 0} z)
{
}
```

### Code fix

```csharp
string[] array = { "" };

Action doNothing = () => { return; };

if (array is { Length: 0 } z)
{
}
```

[sa1012]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1012.md
[sa1013]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1013.md
[fig-NoSpaceBeforeBrace]:
  https://maroontress.github.io/StyleChecker/images/NoSpaceBeforeBrace.png
