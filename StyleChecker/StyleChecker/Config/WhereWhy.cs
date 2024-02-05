namespace StyleChecker.Config;

/// <summary>
/// Represents an error or a warning that contains its location
/// (line:column) in a file and its message.
/// </summary>
public struct WhereWhy
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WhereWhy"/> struct.
    /// </summary>
    /// <param name="line">
    /// The line number.
    /// </param>
    /// <param name="column">
    /// The column number.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    public WhereWhy(int line, int column, string message)
    {
        Line = line;
        Column = column;
        Message = message;
    }

    /// <summary>
    /// Gets the line number.
    /// </summary>
    public int Line { get; }

    /// <summary>
    /// Gets the column number.
    /// </summary>
    public int Column { get; }

    /// <summary>
    /// Gets the message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the tuple representing this object.
    /// </summary>
    /// <returns>
    /// The tuple.
    /// </returns>
    public (int Line, int Column, string Message) ToTuple()
    {
        return (Line, Column, Message);
    }
}
