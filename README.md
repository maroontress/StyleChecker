# StyleChecker

StyleChecker is yet another code style checker like
[StyleCop Analyzers][stylecopanalyzers].
It is intended to be used together with StyleCop Analyzers.

## Rules to check

- Avoid a long line. In default, it allows less than 80 columns,
  but the length can be configured.
- Regulate spacing around a semicolon, specially in `for` statements.
  The style `for (;;)` of an infinite `for` loop is allowed.
  Note that this rule is realized with NoSpaceBeforeSemicolon and
  SpaceAfterSemicolon analyzers, and they are a replacement of
  [SA1002][sa1002].
- Avoid post increment/decrement operators (e.g. `i++`) if they can be
  replaced with pre increment/decrement ones.
- Avoid including an underscore character (`_`) in the identifier of
  local variables.
- Remove unnecessary using directives.
- Use `T` as a type parameter name if the type parameter is single.
- Move type parameters from the static class to its methods if possible.
- Avoid thoughtless names for the identifer of local variables.
- Avoid invoking `ReadByte()` method of `System.IO.BinaryReader` class
  in incremental `for` loops.
- Avoid `using` statements for some types that have no resources to dispose.
- Don't discard the return value of some delicate methods like
  `System.IO.Stream.Read(byte[], int, int)`.
- Remove unused local variables.
- Avoid assignment to the parameters passed _by value_.

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
