# StyleChecker

StyleChecker is yet another code style checker and refactoring tool like
[FxCopAnalyzers][fxcopanalyzers],
[StyleCop Analyzers][stylecopanalyzers],
[SonarLint][sonarlint],
[Roslynator][roslynator],
and so on. It uses
[the .NET Compiler Platform ("Roslyn")](https://github.com/dotnet/roslyn)
to analyze the C# source code of .NET Core projects and outputs diagnostics
of a rule violation,
and when running with Visual Studio it provides code fixes as much as possible.
Note that StyleChecker contains only supplemental or niche analyzers,
so it is intended to be used together with other Roslyn analyzers.

## Get started

StyleChecker is available as
[the ![NuGet-logo][nuget-logo] NuGet package][nuget-stylechecker].

### Install StyleChecker to your project with Visual Studio

1. Open Package Manager Console. (Open your project with Visual Studio, and
   select Tools
   &#x279c; NuGet Package Manager
   &#x279c; Package Manager Console.)
2. Enter the command `Install-Package StyleChecker` in the Package Manager
   Console.

### Install StyleChecker to your project with .NET Core CLI

1. Enter the command `dotnet add package StyleChecker` with the console.

## Diagnostics

See [the list of diagnostics](doc/rules).

## Customize configuration

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

[fxcopanalyzers]: https://github.com/dotnet/roslyn-analyzers
[stylecopanalyzers]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers
[sonarlint]: https://github.com/SonarSource/sonarlint-visualstudio
[roslynator]: https://github.com/JosefPihrt/Roslynator
[nuget-stylechecker]: https://www.nuget.org/packages/StyleChecker/
[nuget-logo]: https://maroontress.github.io/images/NuGet-logo.png
