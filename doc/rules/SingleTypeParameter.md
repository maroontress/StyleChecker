<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# SingleTypeParameter

<div class="horizontal-scroll">

![SingleTypeParameter][fig-SingleTypeParameter]

</div>

## Summary

Use `T` as the type parameter name if the type parameter is single.

## Default severity

Warning

## Description

[Names of Classes, Structs, and Interfaces][names] \[[1](#ref1)\] are cited as
follows:

> ### Names of Generic Type Parameters
>
> - ✓ Consider using `T` as the type parameter name for types with one
>   single-letter type parameter.

However, renaming the type parameter name to `T` may cause a compilation
error/warning or change the meaning, so some cases must be excluded.

> 🛠️ **Changed**
>
> Before version 2.1.0, this analyzer issued diagnostics even if the code fix
> would cause compilation errors or warnings.

### How to decide if renaming is possible

Suppose you want to know if the type parameter `NotT` can be renamed to `T`
without causing a compilation error or warning and changing the meaning.

First, we will discuss the case where a syntax node with the type parameter
`NotT` is a type _C_ (e.g., `class`, `struct`, etc.). We cannot rename `NotT` to
`T` in the following cases:

- The type name of _C_ is `T`.
- One of the members of type _C_ is named `T`.

For example, renaming the type parameter `NotT` to `T` causes a compilation
error as follows:

```cs
// Renaming NotT to T causes an error CS0694 at the line /*💀*/
/*💀*/ public sealed class T<NotT>
{
}
```

```cs
// Renaming NotT to T causes an error CS0102 at the line /*💀*/
public sealed class U<NotT>
{
    // A MemberDeclaration with the name "T" exists in this scope.
    /*💀*/ public void T()
    {
    }
}
```

In the example above, the member named `T` is a method, but it can also be a
field, property, delegate, event, or inner type.

We also cannot rename it to avoid compilation warnings in the following cases:

- The type _C_ has the syntax nodes containing the type parameter `T` inside
  _C_.
- One of the ancestors of type _C_ is the syntax node containing the type
  parameter `T`.

For example:

```cs
// Renaming NotT to T causes a warning CS0693 at the line /*💀*/
public sealed class C<NotT>
{
    public sealed class Inner
    {
        /*💀*/ public static class U<T>
        {
        }
    }

    /*💀*/ public void M<T>()
    {
    }
}
```

```cs
public sealed class U<T>
{
    public sealed class Inner
    {
        // Renaming NotT to T causes a warning CS0693 at the line /*💀*/
        /*💀*/ public static class C<NotT>
        {
        }
    }
}
```

In some cases, we change the meaning of `T` when we rename `NotT` to `T`. If
there is a token named `T` inside type _C_, `NotT` cannot be renamed to `T`
unless `T` is the type name and the declaration of `T` is inside _C_. For
example:

```cs
public sealed class U
{
    public sealed class Inner
    {
        // Renaming NotT to T causes a change the meaning of T in C<NotT>:
        // U.T -> T in C<T>
        public static class C<NotT>
        {
            public static T? Default { get; }
        }
    }

    public sealed class T
    {
    }
}
```

```cs
// Renaming NotT to T does not change the meaning of T in C<NotT>:
public sealed class C<NotT>
{
    public sealed class Inner
    {
        // In this scope, "T" becomes "C<?>.Inner.T", so it is all right.
        public static T Default = new();

        public class T
        {
        }
    }
}
```

Next, we will discuss the case where a syntax node with the type parameter
`NotT` is a member _M_ (e.g., a method, etc.). We cannot rename `NotT` to `T`
in the following cases:

- The member _M_ has a syntax token named `T` inside _M_.
- One of the ancestors of the member _M_ is the syntax node containing the type
  parameter `T`.

For example:

```cs
public sealed class C
{
    // Renaming NotT to T causes an error CS0412 at the line /*💀*/
    public static int M<NotT>()
    {
        /*💀*/ var T = 0;
        return T;
    }
}
```

```cs
public sealed class C
{
    // Renaming NotT to T causes an error CS0412 at the line /*💀*/
    public static void M<NotT>()
    {
        /*💀*/ static void T()
        {
        }

        T();
    }
}
```

```cs
public sealed class C<T>
{
    public sealed class Inner
    {
        // Renaming NotT to T causes a warning CS0693 at the line /*💀*/
        /*💀*/ public static void M<NotT>()
        {
        }
    }
}
```

## Code fix

The code fix provides an option to replace the type parameter name with `T`.

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
