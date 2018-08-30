namespace StyleChecker
{
    /// <summary>
    /// Provides constants for the analyzer's category.
    /// </summary>
    public static class Categories
    {
        /// <summary>
        /// The category of spacing rules.
        /// </summary>
        public const string Spacing = nameof(Spacing);

        /// <summary>
        /// The category of rules for something like size or length.
        /// </summary>
        public const string Size = nameof(Size);

        /// <summary>
        /// The category of ordering rules.
        /// </summary>
        public const string Ordering = nameof(Ordering);

        /// <summary>
        /// The category of naming rules.
        /// </summary>
        public const string Naming = nameof(Naming);

        /// <summary>
        /// The category of settings.
        /// </summary>
        public const string Settings = nameof(Settings);

        /// The category of cleaning rules.
        /// </summary>
        public const string Cleaning = nameof(Cleaning);
    }
}
