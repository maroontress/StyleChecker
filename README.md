# StyleChecker

StyleChecker is yet another code style checker like
[StyleCop Analyzers][stylecopanalyzers].
It is intended to be used together with StyleCop Analyzers.

## Rules

### Cleaning

- [RedundantTypedArrayCreation](doc/rules/RedundantTypedArrayCreation.md)
  &mdash; Use an implicitly-typed array creation.
- [UnusedUsing](doc/rules/UnusedUsing.md) &mdash;
  Remove unnecessary using directives.
- [UnusedVariable](doc/rules/UnusedVariable.md) &mdash;
  Remove unused local variables.

### Naming

- [SingleTypeParameter](doc/rules/SingleTypeParameter.md) &mdash;
  Use `T` as a type parameter name if the type parameter is single.
- [ThoughtlessName](doc/rules/ThoughtlessName.md) &mdash;
  Avoid thoughtless names for the identifer of local variables.
- [Underscore](doc/rules/Underscore.md) &mdash;
  Avoid including an underscore character (`_`) in the identifier of local
  variables.

### Ordering

- [PostIncrement](doc/rules/PostIncrement.md) &mdash;
  Avoid post increment/decrement operators (e.g. `i++`) if they can be
  replaced with pre increment/decrement ones.

### Refactoring

- [AssignmentToParameter](doc/rules/AssignmentToParameter.md) &mdash;
  Avoid assignment to the parameters passed _by value_.
- [DiscardingReturnValue](doc/rules/DiscardingReturnValue.md) &mdash;
  Don't discard the return value of some delicate methods like
  `System.IO.Stream.Read(byte[], int, int)`.
- [EmptyArrayCreation](doc/rules/EmptyArrayCreation.md) &mdash;
  Don't create empty arrays, use `System.Array.Empty<T>` instead.
- [EqualsNull](doc/rules/EqualsNull.md) &mdash;
  Use `is null` pattern matching instead of `==` operator.
- [IneffectiveReadByte](doc/rules/IneffectiveReadByte.md) &mdash;
  Avoid invoking `ReadByte()` method of `System.IO.BinaryReader` class
  in incremental `for` loops.
- [IsNull](doc/rules/IsNull.md) &mdash;
  Use `==` operator with `null` instead of `is null` pattern matching.
- [NotDesignedForExtension](doc/rules/NotDesignedForExtension.md) &mdash;
  Must design a class for inheritance, or else prohibit it.
- [StaticGenericClass](doc/rules/StaticGenericClass.md) &mdash;
  Move type parameters from the static class to its methods if possible.
- [TypeClassParameter](doc/rules/TypeClassParameter.md) &mdash;
  Replace the parameter of methods or local functions with a type parameter,
  if its type is `System.Type` and every argument for it is a `typeof()`
  operator.
- [UnnecessaryUsing](doc/rules/UnnecessaryUsing.md) &mdash;
  Avoid `using` statements for some types that have no resources to dispose.

### Settings

- [InvalidConfig](doc/rules/InvalidConfig.md) &mdash;
  Validate the configuration file `StyleChecker.xml`.

### Size

- [LongLine](doc/rules/LongLine.md) &mdash;
  Avoid a long line. In default, it allows less than 80 columns,
  but the length can be configured.

### Spacing

- [SpaceBeforeSemicolon](doc/rules/SpaceBeforeSemicolon.md),
  [NoSpaceAfterSemicolon](doc/rules/NoSpaceAfterSemicolon.md) &mdash;
  Regulate spacing around a semicolon, specially in `for` statements.
  The style `for (;;)` of an infinite `for` loop is allowed.
  Note that this rule is realized with NoSpaceBeforeSemicolon and
  SpaceAfterSemicolon analyzers, and they are a replacement of
  [SA1002][sa1002].

## Requirements to run

Visual Studio 2017 (15.9) or .NET Core 2.1 (2.1.500)

## Requirements to build

Visual Studio 2017 (15.9)

## Install StyleChecker to your project

### with Visual Studio

1. Open Package Manager Console. (Open your project with Visual Studio, and
   select Tools
   &#x279c; NuGet Package Manager
   &#x279c; Package Manager Console.)
2. Enter the command `Install-Package StyleChecker` in the Package Manager
   Console.
3. Set `all` to the PrivateAssets property. (Open Solution Explorer
   and select your project
   &#x279c; Dependencies
   &#x279c; NuGet
   &#x279c; Click StyleChecker with a right button to open StyleChecker
   Package Properties, and then enter `all` to the `PrivateAssets` field.)

### with .NET Core CLI

1. Enter the command `dotnet add package StyleChecker` with the console.
2. Open your project file (`.csproj`) using a text editor like Visual Studio
   Code.
3. Fix the `PackageReference` element in the project file adding the
   `PrivateAssets` attribute with the `all` value as follows:

```xml
<PackageReference Include="StyleChecker" Version="..." PrivateAssets="all" />
```

## Configuration

Some analyzers can be customized to change their behaviors,
placing the configuration file `StyleChecker.xml` at the project root.
The XML Schema Definition file `config.v1.xsd` of the configuration file
and a sample of the configuration file are provided in the directory
`StyleChecker/StyleChecker/nuget/samples/` of the source tree
(or in `~/.nuget/packages/stylechecker/VERSION/samples/`
if you installed StyleChecker with the NuGet package). Note that
StyleChecker does not use the XML Schema Definition file,
but it helps you edit `StyleChecker.xml` with the text editor
that is able to validate XML documents (for example, Visual Studio IDE,
Visual Studio Code, and so on).

Create your own `StyleChecker.xml` file and place it at your project root,
and add the `AdditionalFiles` element to `.csproj` file in your project
as follows:

```xml
<ItemGroup>
  <AdditionalFiles Include="StyleChecker.xml" />
</ItemGroup>
```

Alternatively, with Visual Studio you can set "C# analyzer additional file"
to Build Action property (This property can be changed from
Solution Explorer
&#x279c; Right Click on the `StyleChecker.xml`
&#x279c; Properties
&#x279c; Advanced
&#x279c; Build Action).

[stylecopanalyzers]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers
[sa1002]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
