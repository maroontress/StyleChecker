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

An opening brace (&lsquo;`{`&rsquo;) or a closing brace (&lsquo;`}`&rsquo;) that
is not the first character on the line must generally be preceded by whitespace.

Note that this analyzer and the [NoSpaceAfterBrace][] analyzer are intended to
be used together and replace [SA1012][] and [SA1013][] with them, allowing us to
write empty braces (`{}`) as follows:

```csharp
Action doNothing = () => {};

if (maybeString is {} s)
{
    ⋮
}
```

### Opening braces after parentheses or brackets

No whitespace is required when an opening brace immediately follows an opening
parenthesis (&lsquo;`(`&rsquo;) or an opening bracket (&lsquo;`[`&rsquo;):

```csharp
var isEmpty = value is ({});
var isSingleEmpty = values is [{}];
```

### Empty brace pairs

No whitespace is required between an opening brace and a closing brace in an
empty brace pair:

```csharp
Action doNothing = () => {};
```

### Interpolations are not analyzed

Braces that are part of an interpolation are not analyzed:

```csharp
var text = $"{value}";
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
[NoSpaceAfterBrace]: NoSpaceAfterBrace.md
