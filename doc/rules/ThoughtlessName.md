# ThoughtlessName

![ThoughtlessName][fig-ThoughtlessName]

## Summary

Avoid giving a name that is too facile or thoughtless to the identifiers of
local variables and parameters.

## Default severity

Warning

## Description

Consider a good name for each local variable.
The name of the identifiers must not be too easy as a typical sample code.
This rule reports the following cases:

- The identifier of local variables and parameters must not be an acronym of
  the type name if it is composed of two letters or more. For example, use
  `b` or `builder` for the identifier whose type is `StringBuilder`,
  rather than `sb`.
- Hungarian notation must not be used. For example, do not use `int iResult`.
- The identifiers specified with the configuration file `StyleChecker.xml`
  must not be used.

You can specify identifiers which are not allowed to use,
with the configuration file `StyleChecker.xml`.
For example, if you would like to make sure that `flag` and `flags` are not
used for identifiers, edit `StyleChecker.xml` file as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<config xmlns="https://maroontress.com/StyleChecker/config.v1">
  ⋮
  <ThoughtlessName>
    <disallow id="flag"/>
    <disallow id="flags"/>
  </ThoughtlessName>
  ⋮
</config>
```

The `ThoughtlessName` element can have `disallow` elements
as its child elements,
and the `id` attribute of the `disallow` element specifies the identifier
that is not allowed to use.

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
public void Method(Stream inputStream)
{
    var sb = new StringBuilder();
    var br = new BinaryReader(inputStream);

    var iResult = "hello".IndexOf('e');
    ⋮
```

## See also

- [_General Naming Conventions_][general-naming-conventions]
  \[[1](#ref1)\]

  > ### Word Choice
  >
  > - ✓ DO NOT use Hungarian notation.
  >
  > ### Using Abbreviations and Acronyms
  >
  > - X DO NOT use abbreviations or contractions as part of identifier names.
  >   For example, use `GetWindow` rather than `GetWin`.
  > - X DO NOT use any acronyms that are not widely accepted, and even if
  >   they are, only when necessary.

## References

<a id="ref1"></a>
[1] [Microsoft, _.NET Framework Design Guidelines_][framework-design-guidelines-microsoft]

[framework-design-guidelines-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
[general-naming-conventions]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions
[fig-ThoughtlessName]:
  https://maroontress.github.io/StyleChecker/images/ThoughtlessName.png
