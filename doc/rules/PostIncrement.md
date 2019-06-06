# PostIncrement

## Summary

Avoid post-increment/decrement operators (e.g. `i++`, `i--`),
if they can be replaced with pre-increment/decrement ones
(e.g. `++i`, `--i`).

## Description

In general, unary operators must be followed by their operand.
If the post-increment/decrement operator doesn't make sense
in evaluating the expression, but only its side effects are needed,
it must be replaced with pre-increment/decrement one.

## Code fix

The code fix provides an option replacing the post-increment/decrement
operator with the pre-increment/decrement one.

## Example

### Diagnostic

```csharp
i++;
```

### Code fix

```csharp
++i;
```
