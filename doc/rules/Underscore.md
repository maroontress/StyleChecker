<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# Underscore

<div class="horizontal-scroll">

![Underscore][fig-Underscore]

</div>

## Summary

Avoid including an underscore character (`_`) in the identifier of
local variables, local functions, and parameters.

## Default severity

Warning

## Description

Don't use underscores in identifies.

## Code fix

The code fix provides an option replacing the identifier with `underscore`
if the identifier contains only `_` (a single underscore character). Otherwise,
it provides an option of eliminating underscores in the identifier and
concatenating words in the camel case style.

## Example

### Diagnostic

```csharp
public void Method(int _param)
{
    var _ = 0;
    var max_retry_count = 100;
    if (TryToGet(out var return_value))
    {
    }
    if (this is object _o)
    {
        ⋮
    }
    void Local_Function()
    {
    }
    ⋮
```

### Code fix

```csharp
public void Method(int param)
{
    var underscore = 0;
    var maxRetryCount = 100;
    if (TryToGet(out var returnValue))
    {
    }
    if (this is object o)
    {
        ⋮
    }
    void LocalFunction()
    {
    }
    ⋮
```

[fig-Underscore]:
  https://maroontress.github.io/StyleChecker/images/Underscore.png
