# UnusedVariable

## Summary

Unused variables must be removed.

## Default severity

Warning

## Description

This rule reports diagnostic information as a warning similar to
[CS0219 (The variable '...' is assigned but its value is never used)][cs0219],
[CA1804 (Remove unused locals)][ca1804] and
[CA1801 (Review unused parameters)][ca1801] but more strictly.
It reports as follows:

- Unused local variables (including [out variable declarations][out-var]
  and [pattern matching][pattern-matching])
- Unused parameters of constructors, methods or local functions, except:
  - `interface`
  - `abstract` methods
  - `extern` methods
  - `partial` methods without the definition
  - `virtual` empty methods
  - parameters annotated with `UnusedAttribute` provided with
    [StyleChecker.Annotations][stylechecker-annotations]
- Parameters annotated with `UnusedAttribute` if the annotation is
  not necessary

Note that it does not report the unused parameters of lambda expressions.

A diagnostic CS0219 is given only when a variable is declared with
a constant initializer and unused. If the initializer has side effects
or potential ones (for example, invoking methods),
CS0219 is not raised as follows:

```csharp
{
    // The following code raises CS0219.
    var a = 0;
    var b = "b";

    // The following code doesn't raise CS0219.
    var c = "" + 0;
    var d = Console.ReadLine();
}
```

(Meanwhile, [CS0168 (The variable '...' is declared but never used)][cs0168]
is given when a variable is declared without an initializer and unused.)

Furthermore, unused parameters of the constructor or method,
and unused variables declared with Out Variable Declarations or
Pattern Matching
do not raise CS0219 as follows:

```csharp
// The following code doesn't raise CS0219.
void Method(int unused, object o, Dictionary<string, string> map)
{
    if (o is string s)
    {
    }
    if (map.TryGetValue("key", out var v))
    {
    }
}
```

The UnusedVariable analyzer reports diagnostics for these codes.

## Code fix

The code fix is not provided. Remove the unused variables or parameters.
Otherwise, add/remove `UnusedAttribute` to/from the parameter.

## Example

### Diagnostic

```csharp
public class Main
{
    public void Local()
    {
        // The following code raises UnusedVariable, but not CS0219.
        var s = "" + 0;
    }

    public void Parameter(int arg)
    {
    }

    public void PatternMatching(object o)
    {
        if (o is string s)
        {
        }
    }

    public void OutVar(Dictionary<string, string> map)
    {
        if (map.TryGetValue("key", out var v))
        {
        }
    }
}
```

[cs0168]:
  https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0168
[cs0219]:
  https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0219
[ca1804]:
  https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1804-remove-unused-locals?view=vs-2017
[ca1801]:
  https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1801-review-unused-parameters?view=vs-2017
[out-var]:
  https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/out-var.md
[pattern-matching]:
  https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/pattern-matching.md
[stylechecker-annotations]:
  https://www.nuget.org/packages/StyleChecker.Annotations/
