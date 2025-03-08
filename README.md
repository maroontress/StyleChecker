# StyleChecker

StyleChecker is another code style checking and refactoring tool similar to
[FxCopAnalyzers][fxcopanalyzers], [StyleCop Analyzers][stylecopanalyzers],
[SonarLint][sonarlint], [Roslynator][roslynator], etc. It uses the
[.NET Compiler Platform ("Roslyn")][roslyn] to analyze the C# source code of
.NET projects and outputs diagnostics of rule violations. And when used with
IDEs such as Visual Studio, it provides as many code fixes as possible. Note
that you should use this tool with other Roslyn analyzers, as it contains only
complementary or niche analyzers.

## Get started

StyleChecker is available as [the ![NuGet-logo][nuget-logo] NuGet
package][nuget-stylechecker].

### Install StyleChecker to your project with Visual Studio

1. Open Package Manager Console. (Open your project with Visual Studio, and
   select Tools &#x279c; NuGet Package Manager &#x279c; Package Manager
   Console.)
2. Enter the command `Install-Package StyleChecker` in the Package Manager
   Console.

### Install StyleChecker to your project with .NET CLI

- Enter the command `dotnet add package StyleChecker` with the console.

## Diagnostics

See [the list of diagnostics](doc/rules).

## Customize configuration

You can customize some analyzers to change their behaviors by placing the
configuration file `StyleChecker.xml` at the project root. The XML Schema
Definition file `config.v1.xsd` of the configuration file and a sample of the
configuration file are available in the directory
`StyleChecker/StyleChecker/nuget/samples/` of the source tree (or in
`~/.nuget/packages/stylechecker/VERSION/samples/` if you installed StyleChecker
with the NuGet package). Note that StyleChecker does not use the XML Schema
Definition file. But it helps you edit `StyleChecker.xml` with the text editor
that can validate XML documents (for example, Visual Studio IDE, Visual Studio
Code, and so on).

Create your own `StyleChecker.xml` file, place it at your project root, and add
the `AdditionalFiles` element to the `.csproj` file in your project as follows:

```xml
<ItemGroup>
  <AdditionalFiles Include="StyleChecker.xml" />
</ItemGroup>
```

Alternatively, in Visual Studio, you can set the value "C# analyzer additional
file" to the Build Action property. You can change this property from Solution
Explorer &#x279c; Right Click on the `StyleChecker.xml` &#x279c; Properties
&#x279c; Advanced &#x279c; Build Action.

[roslyn]: https://github.com/dotnet/roslyn
[fxcopanalyzers]: https://github.com/dotnet/roslyn-analyzers
[stylecopanalyzers]: https://github.com/DotNetAnalyzers/StyleCopAnalyzers
[sonarlint]: https://github.com/SonarSource/sonarlint-visualstudio
[roslynator]: https://github.com/JosefPihrt/Roslynator
[nuget-stylechecker]: https://www.nuget.org/packages/StyleChecker/
[nuget-logo]: https://maroontress.github.io/images/NuGet-logo.png
