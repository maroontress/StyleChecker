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

In addition to the above example, you can also move the initial value to
another method that returns a value such as `var s = Method();`.  Or, if the
separation with another method causes a lot of parameters, you can define the
local function instead of the method;

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

Since you must not repeat the branches depending on the same condition, you
should not write the following code:

```csharp
var x = b ? 1 : 0;
var y = b ? 3 : 1;
```

Instead, you can use a tuple as follows:

```csharp
var (x, y) = b ? (1, 3) : (0, 1);
```

### Initialization with side effects

Let's consider more practical code as follows:

```csharp
public void Baz()
{
    var n = UserInput;
    if (!IsValid(n))
    {
        logger.Warn($"invalid input: {n}");
        n = DefaultValue;
    }
    else
    {
        logger.Info($"input: {n}");
    }
    ...
}
```

> [Run][example-1]

Even if you rewrite the above example with one-shot initialization, you should
not write the following code:

```csharp
public void Baz()
{
    var raw = UserInput;
    var isValid = IsValid(raw);
    if (!isValid)
    {
        logger.Warn($"invalid input: {raw}");
    }
    else
    {
        logger.Info($"input: {raw}");
    }
    var n = isValid ? raw : DefaultValue;
    ...
}
```

The branch depending on `isValid` should be one time as well as the original
code.  You can do so with local functions as follows:

```csharp
public void Baz()
{
    int ValidUserInput(int raw)
    {
        logger.Info($"input: {raw}");
        return raw;
    }

    int InvalidUserInput(int raw)
    {
        logger.Warn($"invalid input: {raw}");
        return DefaultValue;
    }

    var raw = UserInput;
    var supplier = IsValid(raw)
        ? (Func<int, int>)ValidUserInput
        : InvalidUserInput;
    var n = supplier(raw);
    ...
}
```
> [Run][example-2]

Or more simply, you can also use a tuple containing the action of type `Action`
that represents side effects, as follows:

```csharp
public void Baz()
{
    var raw = UserInput;
    var (n, action) = IsValid(raw)
        ? (raw, new Action(() => logger.Info($"input: {raw}")))
        : (DefaultValue, () => logger.Warn($"invalid input: {raw}"));
    action();
    ...
}
```

> [Run][example-3]

Note that this analyzer does not emit diagnostics against the code examples
described in this section.

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
var n = 0;
if (...) {
    ++n;
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
[example-1]:
  https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUH1AZgAINiBhPAbz2LuIAcwBLANwENgBTUzANmJgu7OAHsoAGwCexZlGDEAIlwBm7CBOAA1dhIg8AvMUwBuPLXpEKACjkKIAZy5gAklAYRgASgt0auekDiAFUnV3dPYiNHZzcPYDMA+gBfX2I0pjZOHiERcWliABlRAHMS52IJUvKwKOIoLgB3Iurnay9EwMyObll5ELC4yKpicoTiVKS6DJYengAjUVEJYhcHHQlmOFt+1i8ogD5iAEJrVlkHYgAGDvMp0hJUABZiACF2AC92tP8g+g5alA6qFYhEEmlAswVMRrMc1hsttYoF4fPdAr8/kEqmVnAA6ADq7DAUGsABIAERyDibOB9eIgYhUKDJcm3NGYoFGZRqDTaXT6TqYyaY4hcCROCH0DEiujYmq4twqURkylghlMllskXCzGoTAAThVUHVzNZgroOum9ww6B+kroAOIAGN0HUGs1yNZ0FrMS7ce8vj6/o6nVc3U0bDdzUFQ/7Pu1o5NJgRHq7ijiwNQ0lZnqsoErrHqww59gYjnrDSXozmXoTiYXMMXS+WDdYq3hJkA=
[example-2]:
  https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUH1AZgAINiBhPAbz2LuIAcwBLANwENgBTUzANmJgu7OAHsoAGwCexZlGDEAIlwBm7CBOAA1dhIg8AvMUwBuPLXpEKACjkKIAZy5gAklAYRgASgt0auekDiAFUnV3dPYiNHZzcPYDMA+gBfX2I0pjZOHiERcWliABlRAHMS52IJUvKwKOIoLgB3Iurnay9EwMyObll5ELC4yKpicoTiVKS6DJYengAjUVEJYhcHHQlmOFt+1i8ogD5iAEJrVlkHYgAGDvMp0hJUABZiACF2AC92tP8g+jtiBstqFYhFgDsFGB2I0fPdAr8/kEqmVnAA6NwqUTWAAkACI5PEQMQqFDGslcbc4YjUAB2QTQzqIyZpQIAtwcTZwEHheIQ+kwln0BGI+jImqogDq7DAUBx+KgHK2fUJxNJ5MpIsCtKUqnUmg2+kZf2ZVLoHFqpLq3KGCUFZulxAcEAYDE2FSMayB21JsM19AA/MRrKgAKwAHjsMD6wAOXi91rBdqCRPZumBgzBRqC5vqdSdLrdYGsPqzWswAE45VAiVQoOqs5NAmkMOgfkmcwBjdB1BrNcjWdAaxFd1HvL5Dv6dq49po2G6l+gdq6jz7tBt4E1WMjFFFgajNx4vDFY1CYacOfYGI6nysXrNbl5SmXBs+Oy/XivWO8bvBAA===
[example-3]:
  https://sharplab.io/#v2:C4LgTgrgdgNAJiA1AHwAICYCMBYAUH1AZgAINiBhPAbz2LuIAcwBLANwENgBTUzANmJgu7OAHsoAGwCexZlGDEAIlwBm7CBOAA1dhIg8AvMUwBuPLXpEKACjkKIAZy5gAklAYRgASgt0auekDiAFUnV3dPYiNHZzcPYDMA+gBfX2I0pjZOHiERcWliABlRAHMS52IJUvKwKOIoLgB3Iurnay9EwMyObll5ELC4yKpicoTiVKS6DJYengAjUVEJYhcHHQlmOFt+1i8ogD5iAEJrVlkHYgAGDvMp0hJUABZiACF2AC92tP8g+g5amB2M0jKFYhEEmlAgDiNZYMR2ABjYDMcT7IxrDZbaxAxo+e5/OgAflhuJg9SavHQ1nah0qrTAADo3CpRNYACQAIjk8RAxCouOSnK8IqhhL51mUag02l0+nJtIMRyqZWcjIA6uwwFAOdyoBxNnA+rz+YLhbcCfQkSjxO1OoTUJgAJy6qB8qhQIUWwKTQJpDDoH5iugwxHoOoNZrkazob2EsOM95fON/UNXCOU6M3e1/RFXROfO1pSaTAiPcPFVVgaj+x4vFlsx3phzoo6Ol0tnMPUgvTXa6xN4gtunt6ydvCTIA=
