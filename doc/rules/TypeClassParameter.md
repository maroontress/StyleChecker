<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# TypeClassParameter

<div class="horizontal-scroll">

![TypeClassParameter][fig-TypeClassParameter]

</div>

## Summary

Replace the parameter of methods or local functions with a type parameter
if possible.

## Default severity

Warning

## Description

The parameter of methods or local functions can be replaced with a type
parameter if its type is `System.Type` and every argument for it is a
`typeof()` operator. For example, the local function `Print` has a single
parameter `type`, whose type is `System.Type`, and *all* the invocations of it
are performed with an argument of the `typeof()` operator whose operand is
not a `static` class, as follows:

```csharp
public void PrintTypes()
{
    void Print(Type type)
    {
        Console.WriteLine(type.FullName);
    }

    Print(typeof(string));
    Print(typeof(int));
    ⋮
```

The following code shows the revised version of `Print` where the
parameter `type` is removed but a type parameter `T` is added instead:

```csharp
public void PrintTypes()
{
    void Print<T>()
    {
        var type = typeof(T);
        Console.WriteLine(type.FullName);
    }

    Print<string>();
    Print<int>();
    ⋮
```

Note that this analyzer doesn't report diagnostics if at least one caller
invokes the original version of `Print` with an argument other than the
`typeof()` operator whose operand is not a `static` class, because it is
unable to replace the parameter `type` with a type parameter `T`.

> ### Restriction
>
> This analyzer can only diagnose local functions and private methods
> with the Visual Studio 2019 editor.
> To diagnose non-private methods with Visual Studio 2019,
> perform Build Solution or Analysis ➜ Run Code Analysis.


## Code fix

The code fix provides the option of replacing the parameter with a type
parameter and inserting a local variable declaration to the top of the
method or the local function. The variable name of the inserted declaration
is the same as the name of the removed parameter.

## Example

### Diagnostic

```csharp
private void DoSomething(Type type)
{
    ⋮
}

public void Invoke()
{
    DoSomething(typeof(string));
}
```

### Code fix

```csharp
private void DoSomething<T>()
{
    var type = typeof(T);
    ⋮
}

public void Invoke()
{
    DoSomething<string>();
}
```

> ### Remarks
>
> If a type has both `DoSomething<T>()` and `DoSomething(Type)` methods
> at the same time, the code fix provider renames `DoSomething<T>`
> (to `DoSomething_0<T>`, for example) at first, and then replaces
> `DoSomething(Type)` with `DoSomething<T>()`.

[fig-TypeClassParameter]:
  https://maroontress.github.io/StyleChecker/images/TypeClassParameter.png
