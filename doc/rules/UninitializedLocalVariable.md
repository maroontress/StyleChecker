<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# UninitializedLocalVariable

<div class="horizontal-scroll">

![UninitializedLocalVariable][fig-UninitializedLocalVariable]

</div>

## Summary

Initialize local variables when they are declared.

## Default severity

Warning

## Description

You should assign an initial value to the local variable always when it is
declared.

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
int foo;
string bar;
```

[fig-UninitializedLocalVariable]:
  https://maroontress.github.io/StyleChecker/images/UninitializedLocalVariable.png
