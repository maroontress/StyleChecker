# StyleChecker

StyleChecker is yet another code style checker like
[StyleCop Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers).
It is supposed to be used with StyleCop Analyzers.

## Rules to check

- Avoid a long line. In default, it allows less than 80 columns,
  but the length can be configured.
- Regulate spacing around a semicolon, specially in `for` statements.
  The style `for (;;)` of an infinite `for` loop is allowed.
  Note that this rule is realized with NoSpaceBeforeSemicolon and
  SpaceAfterSemicolon analyzers, and they are a replacement of
  [SA1002](https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1002.md).
- Avoid post increment/decrement operators (e.g. `i++`) if they can be
  replaced with pre increment/decrement ones.
- Avoid including an underscore character (`_`) in the identifier of
  local variables.
- Remove unnecessary using directives.
- Use `T` as a type parameter name if the type parameter is single.
- Move type parameters from the static class to its methods if possible.

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

Alternatively, with Visual Studio you set "C# analyzer additional file"
to Build Action property (This property can change from
Solution Explorer
&rightarrow; Right Click on the `StyleChecker.xml`
&rightarrow; Properties
&rightarrow; Advanced
&rightarrow; Build Action).
