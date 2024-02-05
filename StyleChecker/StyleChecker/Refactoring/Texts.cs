namespace StyleChecker.Refactoring;

using System;
using System.IO;
using System.Text;

/// <summary>
/// Provides a way to transform a text string.
/// </summary>
public static class Texts
{
    /// <summary>
    /// Substitutes a pattern representing a key for
    /// the value to which the specified function maps the key,
    /// which occurs in the specified template text.
    /// </summary>
    /// <remarks>
    /// A pattern representing a key is <c>${key}</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// var template = "${key}";
    /// var text = Texts.Substitute(template, key => key.ToLower());
    /// // text is a string "KEY".
    /// </code>
    /// </example>
    /// <param name="template">
    /// A template text.
    /// </param>
    /// <param name="map">
    /// A function to map a key string to the value string.
    /// </param>
    /// <returns>
    /// The new text string.
    /// </returns>
    public static string Substitute(
        string template, Func<string, string> map)
    {
        var @in = new StringReader(template);
        int Read() => @in.Read();
        char ReadChar()
        {
            var c = @in.Read();
            if (c == -1)
            {
                throw new EndOfStreamException();
            }
            return (char)c;
        }

        var b = new StringBuilder(template.Length);
        for (;;)
        {
            var o = Read();
            if (o == -1)
            {
                break;
            }
            var c = (char)o;
            if (c != '$')
            {
                b.Append(c);
                continue;
            }
            c = ReadChar();
            if (c != '{')
            {
                b.Append('$');
                b.Append(c);
                continue;
            }
            var keyBuilder = new StringBuilder();
            for (;;)
            {
                c = ReadChar();
                if (c == '}')
                {
                    break;
                }
                keyBuilder.Append(c);
            }
            var key = keyBuilder.ToString();
            b.Append(map(key));
        }
        return b.ToString();
    }
}
