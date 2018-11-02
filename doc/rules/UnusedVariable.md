# UnusedVariable

## Summary

Unused variables must be removed.

## Description

This rule reports diagnostic information as a warning similar to
[CS0219 (The variable '...' is assigned but its value is never used)](https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0219),
[CA1804 (Remove unused locals)](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1804-remove-unused-locals?view=vs-2017)
and
[CA1801 (Review unused parameters)](https://docs.microsoft.com/en-us/visualstudio/code-quality/ca1801-review-unused-parameters?view=vs-2017)
but more strictly.
It reports as follows:

- Unused local variables (including [out variable declarations](https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/out-var.md)
  and
  [pattern matching](https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.0/pattern-matching.md))
- Unused parameters of constructors or methods, except `interface` methods,
  `abstract` methods, `extern` methods, `partial` empty methods and
  `virtual` empty methods, or except ones annotated with `UnusedAttribute`
  provided with [StyleChecker.Annotations](https://www.nuget.org/packages/StyleChecker.Annotations/).
- Parameters that is annotated with `UnusedAttribute` but the annotation is
  not necessary.

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

(Meanwhile,
[CS0168 (The variable '...' is declared but never used)](https://docs.microsoft.com/en-us/dotnet/csharp/misc/cs0168)
is given when a variable is declared without an initializer and unused.)

Furthermore, unused parameters of the constructor or method,
and unused variables declared with out variable declarations or
pattern matching
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

The code fix is not provided. Remove the unused variables or parameters,
otherwise add/remove `UnusedAttribute` to/from the paramter.

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