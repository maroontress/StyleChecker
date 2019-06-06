# UnusedUsing

## Summary

Unused `using` directives must be removed.

## Description

This rule reports diagnostic information of CS8019 (unnecessary using
directive) as a warning.

Note that CS8019 reports diagnostics for auto-generated files,
but this analyzer doesn't.

## Code fix

The code fix is not provided by StyleChecker but provided by Visual Studio IDE.

## Example

### Diagnostic

```csharp
using System;

public class Main
{
    public void Method()
    {
    }
}
```
