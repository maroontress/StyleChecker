# SingleTypeParameter

## Summary

Use `T` as a type parameter name if the type parameter is single.

## Description

The section **Names of Generic Type Parameters** of _.NET Framework
Design Guidelines_ is quoted as follows:

> Consider using `T` as the type parameter name for types with one
single-letter type parameter.

## Code fix

The code fix provides an option replacing the type parameter name with `T`.

## Example

### Diagnostic

```csharp
    public sealed class Code<Type>
    {
        public Code(Type obj)
        {
        }
    }
```

### Code fix

```csharp
    public sealed class Code<T>
    {
        public Code(T obj)
        {
        }
    }
```
