# ThoughtlessName

## Summary

Avoid giving a name that is too facile or thoughtless to the identifiers of
local variables and parameters.

## Description

Consider a good name for each local variable.
The name of the identifiers must not be too easy like a typical sample code.
This rule reports following cases:

- The identifier of local variables and parameters must not be an acronym of
  the type name if it is composed of two letters or more. For example, use
  `b` or `builder` rather than `sb` for the identifier whose type is
  `StringBuilder`.
- Hungarian notation must not be used. For example, do not use `int iResult`.

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
public void Method()
{
    var sb = new StringBuilder();
    var br = new BinaryReader(...);

    var iResult = "hello".IndexOf('e');
}
```

## See also

- [_General Naming Conventions_][general-naming-conventions]\[[1](#ref1)\]

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

<a id="#ref1"></a>
[1] [Microsoft, _.NET Framework Design Guidelines_][framework-design-guidelines-microsoft]

[framework-design-guidelines-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
[general-naming-conventions]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions
