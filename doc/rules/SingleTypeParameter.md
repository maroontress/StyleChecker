<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# SingleTypeParameter

<div class="horizontal-scroll">

![SingleTypeParameter][fig-SingleTypeParameter]

</div>

## Summary

Use `T` as a type parameter name if the type parameter is single.

## Default severity

Warning

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
    public Code(Type instance)
    {
        Instance = instance;
    }

    public Type Instance { get; }
}
```

### Code fix

```csharp
public sealed class Code<T>
{
    public Code(T instance)
    {
        Instance = instance;
    }

    public T Instance { get; }
}
```

## References

<a id="ref1"></a>
[1] [Microsoft, _.NET Framework Design Guidelines_][framework-design-guidelines-microsoft]

[framework-design-guidelines-microsoft]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/
[names]:
  https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/names-of-classes-structs-and-interfaces
[fig-SingleTypeParameter]:
  https://maroontress.github.io/StyleChecker/images/SingleTypeParameter.png
