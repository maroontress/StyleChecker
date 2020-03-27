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

### Discards

This analyzer ignores discards [[1](#ref1)].
So it does not emit diagnostics to the following code:

```csharp
// standalone: be ignored as long as no identifier named '_' is in scope.
 _ = "hello".Length;

// tuple
(int, int) NewPoint(int x, int y) => (x, y);
var (one, _) = NewPoint(1, 2);

// out parameter
void Out(out int x) => x = 3;
Out(out _);

// pattern matching (is)
if ("hello" is string _)
{
    ...
}

// pattern matching (switch)
switch ("hello")
{
case string _:
    break;
...
}
```

## Lambda parameters

This analyzer also checks the input parameters of lambda expressions
[[2](#ref2)]. So it emits diagnostics to the following code:

```csharp
Func<int, int, int> f = (a, _) => a; 
```

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

## References

<a id="ref1"></a>
[1] [Microsoft, _Discards (C# Reference)_][microsoft:csharp-discard]

<a id="ref2"></a>
[2] [Microsoft, _Lambda expressions (C# Programming Guide)_][microsoft:csharp-lambda-expression]

[fig-Underscore]:
  https://maroontress.github.io/StyleChecker/images/Underscore.png
[microsoft:csharp-discard]:
  https://docs.microsoft.com/en-us/dotnet/csharp/discards
[microsoft:csharp-lambda-expression]:
  https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/lambda-expressions
