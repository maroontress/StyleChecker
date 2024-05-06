#pragma warning disable RS1012

namespace StyleChecker.Settings;

using System;
using System.IO;
using System.Linq;
using Maroontress.Oxbind;
using Maroontress.Util;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Config;

/// <summary>
/// Provides the configuration data.
/// </summary>
public static class ConfigBank
{
    /// <summary>
    /// The name of configuration file.
    /// </summary>
    public const string Filename = "StyleChecker.xml";

    private static readonly RootConfig DefaultRootConfig = new();

    private static readonly WeakValueMap<string, ConfigPod> PodMap = new();

    /// <summary>
    /// Gets the configuration pod associated with the specified context.
    /// </summary>
    /// <param name="context">
    /// The context.
    /// </param>
    /// <returns>
    /// The configuration pod.
    /// </returns>
    public static ConfigPod LoadRootConfig(
        CompilationStartAnalysisContext context)
    {
        if (LoadConfigFile(context) is not {} tuple)
        {
            return new ConfigPod(DefaultRootConfig, null, null);
        }
        var (path, source) = tuple;
        lock (PodMap)
        {
            if (PodMap.Get(source) is {} pod)
            {
                return pod;
            }
            var newPod = NewRootConfig(path, source);
            PodMap.Put(source, newPod);
            return newPod;
        }
    }

    /// <summary>
    /// Register the action invoked with the configuration pod, when the
    /// compilation starts.
    /// </summary>
    /// <param name="context">
    /// The context.
    /// </param>
    /// <param name="action">
    /// The action that supplies the configuration pod.
    /// </param>
    public static void LoadRootConfig(
        AnalysisContext context,
        Action<CompilationStartAnalysisContext, ConfigPod> action)
    {
        context.RegisterCompilationStartAction(
            c => action(c, LoadRootConfig(c)));
    }

    private static ConfigPod NewRootConfig(string path, string source)
    {
        try
        {
            var reader = new StringReader(source);
            var factory = new OxbinderFactory();
            var decoder = factory.Of<RootConfig>();
            var rootConfig = decoder.NewInstance(reader);
            return new ConfigPod(rootConfig, null, path);
        }
        catch (Exception e)
        {
            return new ConfigPod(DefaultRootConfig, e, path);
        }
    }

    private static (string Path, string Source)? LoadConfigFile(
        CompilationStartAnalysisContext context)
    {
        var cancellationToken = context.CancellationToken;
        if (context.Options
                .AdditionalFiles
                .FirstOrDefault(f => Path.GetFileName(f.Path) is Filename)
                is not {} configFile
            || configFile.GetText(cancellationToken) is not {} text)
        {
            return null;
        }
        var path = configFile.Path;
        var writer = new StringWriter();
        text.Write(writer, cancellationToken);
        var source = writer.ToString();
        return (path, source);
    }
}
