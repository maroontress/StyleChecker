# SingleTypeParameter

## Summary

Use `T` as a type parameter name if the type parameter is single.

## Description

[Names of Classes, Structs, and Interfaces][names]
\[[1](#ref1)\] is quoted as follows:

> ### Names of Generic Type Parameters
>
> - ✓ Consider using `T` as the type parameter name for types with one
>   single-letter type parameter.

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

## References

<a id="ref1"></a>
[1] [Microsoft, _.NET Framework Design Guidelines_][framework-design-guidelines-microsoft]

[framework-design-guidelines-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
[names]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces
