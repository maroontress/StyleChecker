<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NotOneShotInitialization

<div class="horizontal-scroll">

![NotOneShotInitialization][fig-NotOneShotInitialization]

</div>

## Summary

Declare local variables with one-shot initialization.

## Default severity

Warning

## Description

Do **NOT**:

- Declare a local variable and initialize it with a provisional value
- And then, depending on some condition, assign another value to that variable

For example, see the following code:

```csharp
public const int MaxScore = 100;

public void Foo(int score)
{
    var s = score;
    if (s > MaxScore)
    {
        s = MaxScore;
    }
    ...
}
```

Walter E.  Brown calls this **the &ldquo;let me change my mind&rdquo; idiom**
in \[[1](#ref1)\].

You should rewrite it with _one-shot initialization_ as follows:

```cs
...
var s = (score > MaxScore) ? MaxScore : score;
...
```

or

```cs
...
var s = Math.Min(score, MaxScore);
...
```

Let's see the next example:

```cs
public void Bar(string colorName)
{
    var c = -1;
    switch (colorName)
    {
        case "black":
            c = 0;
            break;
        case "blue":
            c = 1;
            break;
        case "red":
            c = 2;
            break;
        ...
    }
    ...
}
```

We can rewrite it with the `switch` expression as follows:

```cs
...
var c = colorName switch
{
    "black" => 0,
    "blue" => 1,
    "red" => 2,
    ...
    _ => -1,
};
...
```

or with `Dictionary`:

```cs
private static readonly Dictionary<string, int> colorMap
    = new Dictionary<string, int>()
{
    ["black"] = 0,
    ["blue"] = 1,
    ["red"] = 2,
    ...
};

public void Bar(string colorName)
{
    var c = colorMap.GetValueOrDefault(colorName, -1);
    // If you cannot use the 'GetValueOrDefault' method, try the following
    // code instead:
    //
    // var c = colorMap.TryGetValue(colorName, out var found) ? found : -1;
    ...
}
```

In addition to the above example, you can also separate the initial value into
another method that returns a value such as `var s = Method();`.  Or, if the
separation causes a lot of parameters, you can define the local function
instead of the method;

### Initializing multiple local variables

Consider the following example:

```csharp
bool b = ...;

var x = 0;
var y = 1;
if (b) {
    x = 1;
    y = 3;
}
```

Since you must avoid repeating the same branch, you should not write the
following code:

```csharp
var x = b ? 1 : 0;
var y = b ? 3 : 1;
```

Instead, you can use a tuple as follows:

```csharp
var (x, y) = b ? (1, 3) : (0, 1);
```

## Code fix

The code fix is not provided.

## Example

### Diagnostic

```csharp
var b = 1;
if (...) {
    b = 3;
}
```

```csharp
var v = 0;
switch (...)
{
    case ...:
        v = 1;
        break;
    case ...:
        v = 2;
        break;
}
```

## References

<a name="ref1"></a>
\[1\] [Walter E. Brown. _Crazy Code, Crazy Coders_, Meeting C++ 2019][crazy-code-crazy-coders]

[fig-NotOneShotInitialization]:
  https://maroontress.github.io/StyleChecker/images/NotOneShotInitialization.png
[crazy-code-crazy-coders]:
  https://isocpp.org/blog/2019/12/crazy-code-crazy-coders-walter-e.-brown-closing-keynote-meeting-cpp-2019
