<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# StinkyBooleanExpression

<div class="horizontal-scroll">

![StinkyBooleanExpression][fig-StinkyBooleanExpression]

</div>

## Summary

Do not use the conditional operator (`?:`)
where either the second or third operand is a `bool` literal
(`true` or `false`), resulting in `bool` values.

## Default severity

Warning

## Description

There are some stinky boolean expressions with a conditional
operator that can be replaced with the `&&` (conditional logical
AND) or `||` (conditional logical OR) operator, as follows:

```csharp
(b1 ? b2 : false)
(b1 ? true : b2)
```

where the type of `b1` and `b2` is `bool`. It is possible to
replace the former conditional expression with `b1 && b2`, the
latter with `b1 || b2`.

### Remarks

The diagnostics IDE0057 providing
_Simplify conditional expressions refactoring_,
which is available with
[Visual Studio 2019 version 16.6 preview
2][microsoft:vs2019-v16.6-preview-2],
includes the same feature as this analyzer.

## Code fix

The code fix provides an option replacing the conditional
operator with the `&&` or `||` operator.
However, if the diagnostics IDE0057 provides an option
"Simplify conditional expression" with Visual Studio,
you should use it.

## Example

### Diagnostic

```csharp
_ = (b1 ? b2 : false);
_ = (b1 ? false : b2);

_ = (b1 ? true : b2);
_ = (b1 ? b2 : true);
```

### Code fix

```csharp
_ = ((b1) && (b2));
_ = (!(b1) && (b2));

_ = ((b1) || (b2));
_ = (!(b1) || (b2));
```

[microsoft:vs2019-v16.6-preview-2]:
  https://docs.microsoft.com/en-us/visualstudio/releases/2019/release-notes-preview#16.6.0-pre.2.1
[fig-StinkyBooleanExpression]:
  https://maroontress.github.io/StyleChecker/images/StinkyBooleanExpression.png
