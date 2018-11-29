# StyleChecker

StyleChecker is yet another code style checker like
[StyleCop Analyzers][stylecopanalyzers].
It is intended to be used together with StyleCop Analyzers.

## Rules

### Cleaning

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
  Avoid including an underscore character (`_`) in the identifier of local variables.

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
- [IneffectiveReadByte](doc/rules/IneffectiveReadByte.md) &mdash;
  Avoid invoking `ReadByte()` method of `System.IO.BinaryReader` class
  in incremental `for` loops.
- [NotDesignedForExtension](doc/rules/NotDesignedForExtension.md) &mdash;
  Must design a class for inheritance, or else prohibit it.
- [StaticGenericClass](doc/rules/StaticGenericClass.md) &mdash;
  Move type parameters from the static class to its methods if possible.
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

- [NoSpaceBeforeSemicolon](doc/rules/NoSpaceBeforeSemicolon.md),
  [SpaceAfterSemicolon](doc/rules/SpaceAfterSemicolon.md) &mdash;
  Regulate spacing around a semicolon, specially in `for` statements.
  The style `for (;;)` of an infinite `for` loop is allowed.
  Note that this rule is realized with NoSpaceBeforeSemicolon and
  SpaceAfterSemicolon analyzers, and they are a replacement of
  [SA1002][sa1002].

## Configuration

- Place the configuration file `StyleChecker.xml` as follows in the
  project root.

```xml
<config
    maxLineLength="80"
    />
```

- Add the `AdditionalFiles` element to `.csproj` file in your project
  as follows:

```xml
  <ItemGroup>
    <AdditionalFiles Include="StyleChecker.xml" />
  </ItemGroup>
```

Alternatively, with Visual Studio you can set "C# analyzer additional file"
to Build Action property (This property can be changed from
Solution Explorer
&rightarrow; Right Click on the `StyleChecker.xml`
&rightarrow; Properties
&rightarrow; Advanced
&rightarrow; Build Action).

[stylecopanalyzers]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers
[sa1002]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md
