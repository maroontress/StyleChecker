namespace Maroontress.Oxbind
{
    using Maroontress.Util;

    /// <summary>
    /// Metadata for the class corresponding to the XML element, that
    /// represents the relationship between the element and its child elements.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="Schema"/> object is the immutable list of <see
    /// cref="SchemaType"/>, and defines the possible order and number of
    /// occurrences of the child elements.
    /// </para>
    /// <para>
    /// The class corresponding to the XML element that has the child elements
    /// must have the single, <c>static</c> and <c>readonly</c> <see
    /// cref="Schema"/> field annotated with <see
    /// cref="ElementSchemaAttribute"/>. For example, the root element
    /// <c>movie</c> has its child elements <c>director</c> and <c>cast</c> as
    /// follows:
    /// </para>
    /// <code class="lang-xml">
    /// &lt;movie name="Avatar"&gt;
    ///   &lt;director name="James Cameron"/&gt;
    ///   &lt;cast name="Sam Worthington"/&gt;
    ///   &lt;cast name="Zoe Saldana"/&gt;
    /// &lt;/movie&gt;
    /// </code>
    /// <para>
    /// The <c>Movie</c> class that stands for the element <c>movie</c>
    /// should be as follows:
    /// </para>
    /// <code>
    /// [ForElement("movie")]
    /// public sealed class Movie
    /// {
    ///     [ElementSchema]
    ///     private static readonly Schema TheSchema = Schema.Of(
    ///         Mandatory.Of&lt;Director&gt;(),
    ///         Multiple.Of&lt;Cast&gt;());
    ///
    ///     [ForAttribute("name")]
    ///     private String name;
    ///     ...
    /// }
    /// </code>
    /// <para>
    /// Likewise, The <c>Director</c> and <c>Cast</c> classes that stand for
    /// the elements <c>director</c> and <c>cast</c> respectively should be as
    /// follows:
    /// </para>
    /// <code>
    /// [ForElement("director")]
    /// public class Director { ... }
    ///
    /// [ForElement("cast")]
    /// public class Cast { ... }
    /// </code>
    /// <para>
    /// All classes that the <see cref="Schema"/> object contains must be
    /// handled in that class with the field annotated with <see
    /// cref="ForChildAttribute"/> or the method annotated with
    /// <see cref="FromChildAttribute"/>.
    /// </para>
    /// <para>
    /// The definition of the <see cref="Schema"/> object does not allow the
    /// child element corresponding to the class <c>T</c> to appear two or more
    /// times.
    /// </para>
    /// </remarks>
    /// <seealso cref="ElementSchemaAttribute"/>
    /// <seealso cref="ForChildAttribute"/>
    /// <seealso cref="FromChildAttribute"/>
    /// <seealso cref="SchemaType"/>
    /// <seealso cref="Mandatory.Of{T}()"/>
    /// <seealso cref="Multiple.Of{T}()"/>
    /// <seealso cref="Optional.Of{T}()"/>
    public sealed class Schema
    {
        /// <summary>
        /// An empty <see cref="Schema"/> object.
        /// </summary>
        /// <example>
        /// <para>
        /// If the class has no <c>static</c> and <c>readonly</c>
        /// <see cref="Schema"/> field annotated with
        /// <see cref="ElementSchemaAttribute"/>, it is considered to be
        /// containing no child elements. So, for example,
        /// the <c>MySchema</c> field in the <c>Movie</c> class as follows
        /// can be omitted.
        /// </para>
        /// <code>
        /// [ForElement("director")]
        /// public class Director
        /// {
        ///     [ElementSchema]
        ///     private static readonly Schema MySchema = Schema.EMPTY;
        ///     ...
        /// }
        /// </code>
        /// </example>
        public static readonly Schema Empty = new Schema(new SchemaType[0]);

        /// <summary>
        /// <see cref="SchemaType"/> objects representing this schema.
        /// </summary>
        private readonly ImmutableArray<SchemaType> types;

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class
        /// with the specified <see cref="SchemaType"/> objects.
        /// </summary>
        /// <param name="args">
        /// The array of <see cref="SchemaType"/> objects.
        /// </param>
        private Schema(SchemaType[] args)
        {
            types = new ImmutableArray<SchemaType>(args);
        }

        /// <summary>
        /// Creates a new instance with the specified <see cref="SchemaType"/>
        /// objects.
        /// </summary>
        /// <remarks>
        /// If the <paramref name="args"/> is empty,
        /// returns <see cref="Empty"/>.
        /// </remarks>
        /// <param name="args">
        /// <see cref="SchemaType"/> objects.
        /// </param>
        /// <returns>
        /// A new <see cref="Schema"/> object.
        /// </returns>
        public static Schema Of(params SchemaType[] args)
        {
            return (args.Length == 0) ? Empty : new Schema(args);
        }

        /// <summary>
        /// Gets the immutable array of the <see cref="SchemaType"/>
        /// object.
        /// </summary>
        /// <returns>
        /// The immutable array of the <see cref="SchemaType"/> object.
        /// </returns>
        public ImmutableArray<SchemaType> Types() => types;
    }
}
