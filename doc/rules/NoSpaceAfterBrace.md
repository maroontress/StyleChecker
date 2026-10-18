<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NoSpaceAfterBrace

<div class="horizontal-scroll">

![NoSpaceAfterBrace][fig-NoSpaceAfterBrace]

</div>

## Summary

A brace (&lsquo;`{`&rsquo; or &lsquo;`}`&rsquo;) must be followed by a white
space.

## Default severity

Warning

## Description

An opening brace (&lsquo;`{`&rsquo;) or a closing brace (&lsquo;`}`&rsquo;) that
is not the last character on the line must generally be followed by whitespace.

Note that this analyzer and the [NoSpaceBeforeBrace][] analyzer are intended to
be used together and replace [SA1012][] and [SA1013][] with them, allowing us to
write empty braces (`{}`) as follows:

```csharp
Action doNothing = () => {};

if (maybeString is {} s)
{
    ⋮
}
```

### Empty brace pairs

No whitespace is required after an opening brace when it is immediately
followed by a closing brace:

```csharp
Action doNothing = () => {};
```

### Closing braces before specific tokens

No whitespace is required after a closing brace when it is immediately followed
by a closing parenthesis (&lsquo;`)`&rsquo;), a closing bracket
(&lsquo;`]`&rsquo;), an opening bracket (&lsquo;`[`&rsquo;), a comma
(&lsquo;`,`&rsquo;), a period (&lsquo;`.`&rsquo;), an exclamation mark
(&lsquo;`!`&rsquo;), a question mark (&lsquo;`?`&rsquo;), or a semicolon
(&lsquo;`;`&rsquo;) that terminates a statement other than an empty statement.

```csharp
// } followed by )
M(new[] { "" });

// } followed by [
_ = new[] { "" }[0];

// } followed by ]
var isSingleEmpty = array is [{}];

// } followed by ,
_ = new[] { new[] { "" }, };

// } followed by .
_ = new[] { "" }.ToString();

// } followed by ;
Action doNothing = () => {};
```

### Interpolations are not analyzed

Braces that are part of an interpolation are not analyzed.

```csharp
var text = $"{value}";
```

## Code fix

The code fix provides an option inserting a space after the brace.

## Example

### Diagnostic

```csharp
string[] array = {"" };

Action doNothing = () => {return; };

if (array is {Length: 0 }z)
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

[SA1012]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1012.md
[SA1013]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1013.md
[fig-NoSpaceAfterBrace]:
  https://maroontress.github.io/StyleChecker/images/NoSpaceAfterBrace.png
[NoSpaceBeforeBrace]: NoSpaceBeforeBrace.md
