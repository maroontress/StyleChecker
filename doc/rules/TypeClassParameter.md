# TypeClassParameter

## Summary

Replace the parameter of methods or local functions with a type parameter
if possible.

## Description

The parameter of methods or local functions can be replaced with a type
parameter, if its type is `System.Type` and every argument for it is a
`typeof()` operator. For example, the local function `Print` has the single
paramter `type`, whose type is `System.Type`, and all the invocations of it
are performed with an arguement of a `typeof()` operator, as follows:

```csharp
public void PrintTypes()
{
    void Print(Type type)
        => Console.WriteLine(type.FullName);

    Print(typeof(string));
    Print(typeof(int));
    ⋮
}
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
}
```

Note that this analyzer doesn't report diagnostics if at least one caller
invokes the original version of `Print` with an argument other than a
`typeof()` operator, because it is unable to replace the parameter `type`
with a type parameter `T`.

## Code fix

The code fix provides the option of replacing the parameter with a type
parameter, and inserting a local variable declaration to the top of the
method or local function. The variable name of the inserted declaration
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
    DoSomething<string>());
}
```
