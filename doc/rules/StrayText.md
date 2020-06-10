<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# StrayText

<div class="horizontal-scroll">

![StrayText][fig-StrayText]

</div>

## Summary

Text in the [Documentation Comments][microsoft:csharp-documentation-comments]
\[[1](#ref1)\] must be inside any XML tag.

## Default severity

Warning

## Description

The documentation comments must not have the top-level text,
which is not in the XML tag.
Such a stray text should be fixed.

For example, I have seen the following code in a certain OSS project:

```csharp
/// This class is...
public class DocumentCommentIsNotInsideTag
{
    ⋮
}
```

The author probably missed the `summary` tag.

Similarly, I have also seen the following code:

```csharp
/// <summary>
/// ...
/// </summary>
/// public
public void TextIsOutsideTag()
{
    ⋮
}
```

The text '`public`' in the documentation comments is outside the tags.
Perhaps the author forgot to delete the text.

## Code fix

The code fix is not provided.
Surround the text with the proper tags,
or move the text to the right location.

## Example

### Diagnostic

```csharp
/// Not in the tag.
private void NotInTheTag()
{
}

/// Before the tag.
/// <summary>
/// </summary>
/// After the tag.
private void OutsideTheTag()
{
}

/// <summary>
/// </summary>
/// Between tags.
/// <param name="x">
/// </param>
private void BetweenTags(int x)
{
}
```

## References

<a id="ref1"></a>
[1] [Microsoft, _C# Language Reference_][microsoft:csharp-documentation-comments]

[fig-StrayText]:
  https://maroontress.github.io/StyleChecker/images/StrayText.png
[microsoft:csharp-documentation-comments]:
  https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/documentation-comments
