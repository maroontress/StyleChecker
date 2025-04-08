namespace Analyzers.Cleaning.ByteOrderMark;

/// <summary>
/// The default implementation of <see cref="Toolkit"/> class.
/// </summary>
public sealed class DefaultToolkit : Toolkit
{
    /// <inheritdoc/>
    public override DirectoryAct GetDirectoryAct(string path)
    {
        return new DefaultDirectoryAct(path);
    }
}
