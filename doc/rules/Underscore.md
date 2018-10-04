# Underscore

## Summary

Avoid including an underscore character (`_`) in the identifier of
local variables, local functions, parameters.

## Description

Don't use underscores in identifies.

## Code fix

The code fix provides an option eliminating underscores in the identifier,
and concatenating words in the camel case style.

## Example

### Diagnostic

```csharp
public void Method(int _param)
{
    int max_retry_count = 100;
    if (tryToGet(out var return_value))
    {
    }
    void local_function()
    {
    }
}
```

### Code fix

```csharp
public void Method(int param)
{
    int maxRetryCount = 100;
    if (tryToGet(out var returnValue))
    {
    }
    void localFunction()
    {
    }
}
```
