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

## References

The section
**[General Naming Conventions](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/general-naming-conventions)**
of _.NET Framework Design Guidelines_ is quoted as follows:

> ### Word Choice
>
> - DO NOT use Hungarian notation.
>
> ### Using Abbreviations and Acronyms
>
> - DO NOT use abbreviations or contractions as part of identifier names.
>   For example, use `GetWindow` rather than `GetWin`.
> - DO NOT use any acronyms that are not widely accepted, and even if they are,
>   only when necessary.

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
