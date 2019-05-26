# NoDocumentation

## Summary

A replacement for [CS1591][cs1591] (Missing XML comment for
publicly visible type or member), [SA1600][sa1600] (Elements should
be documented), and so on.

## Description

### To enable the diagnostics for your project

Note that this diagnostics is emitted
only when [`/doc` compiler option][doc-compiler-option] is specified.
If you use Visual Studio 2019, set this compiler option as follows:

> 1. Open the project's Properties page (Project &#x279c; _project name_
>    Properties...)
> 2. Click the Build tab
> 3. Modify the XML documentation file property (turn on checkbox and
>    specify the path)

### To ignore something with the specified attribute

This analyzer can be configured to ignore diagnostics, for example,
for test methods like following code:

```csharp
[TestMethod]
public void Foo()
{
  ⋮
}
```

To do so, edit `StyleChecker.xml` file as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<config xmlns="https://maroontress.com/StyleChecker/config.v1">
  ⋮
  <NoDocumentation>
    <ignore with="Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute" />
  </NoDocumentation>
  ⋮
</config>
```

Further more, for example, if you would like ignore test classes wholly
(namely, ignore them and all they contain) like following code:

```csharp
[TestClass]
public sealed class FooTest
{
  ⋮
}
```

To do so, edit `StyleChecker.xml` file as follows:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<config xmlns="https://maroontress.com/StyleChecker/config.v1">
  ⋮
  <NoDocumentation>
    <ignore with="Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute"
            inclusive="true" />
  </NoDocumentation>
  ⋮
</config>
```

## Code fix

The code fix is not provided.

[cs1591]:
  https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591
[sa1600]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1600.md
[doc-compiler-option]:
  https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/doc-compiler-option
