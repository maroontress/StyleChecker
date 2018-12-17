namespace Maroontress.Oxbind.Impl
{
    using System;
    using System.Text;

    /// <summary>
    /// An abstraction of the validator.
    /// </summary>
    public abstract class AbstractValidator
    {
        /// <summary>
        /// The resource bundle.
        /// </summary>
        private readonly Func<string, string> bundle;

        /// <summary>
        /// The buffer to store log messages.
        /// </summary>
        private readonly StringBuilder builder = new StringBuilder();

        /// <summary>
        /// Indicates whether this validator has detected errors.
        /// </summary>
        private bool hasError = false;

        /// <summary>
        /// The action to record an error message.
        /// </summary>
        private Action<string, object[]> errorAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="AbstractValidator"/>
        /// class.
        /// </summary>
        /// <param name="label">
        /// The prefix for each log message.
        /// </param>
        protected AbstractValidator(string label)
        {
            var manager = Resource.ResourceManager;
            var culture = Resource.Culture;

            Label = label;
            bundle = s => manager.GetString(s, culture);

            Action<string, object[]> secondErrorAction
                = (m, a) => Log("error", m, a);
            errorAction = (m, a) =>
            {
                secondErrorAction(m, a);
                hasError = true;
                errorAction = secondErrorAction;
            };
        }

        /// <summary>
        /// Gets a value indicating whether this validator has detected errors.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this validator has detected errors.
        /// </returns>
        public bool IsValid => !hasError;

        /// <summary>
        /// Gets the prefix for each log message.
        /// </summary>
        private string Label { get; }

        /// <summary>
        /// Get a new log messages representing the warnings/errors that
        /// this validator has detected.
        /// </summary>
        /// <returns>
        /// The log messages.
        /// </returns>
        public string GetMessage()
        {
            return builder.ToString();
        }

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">
        /// The key of the resource bundle representing the pattern of the
        /// formatted string.
        /// </param>
        /// <param name="args">
        /// The arguments for the formatted string.
        /// </param>
        protected void Warn(string message, params object[] args)
            => Log("warning", message, args);

        /// <summary>
        /// Logs an error message and marks this validator to indicate that it
        /// has detected errors.
        /// </summary>
        /// <param name="message">
        /// The key of the resource bundle representing the pattern of the
        /// formatted string.
        /// </param>
        /// <param name="args">
        /// The arguments for the formatted string.
        /// </param>
        protected void Error(string message, params object[] args)
            => errorAction(message, args);

        /// <summary>
        /// Appends a log message to the buffer.
        /// </summary>
        /// <param name="type">
        /// <c>"warning"</c> or <c>"error"</c>.
        /// </param>
        /// <param name="message">
        /// The key of the resource bundle representing the pattern of the
        /// formatted string.
        /// </param>
        /// <param name="args">
        /// The arguments for the formatted string.
        /// </param>
        private void Log(string type, string message, params object[] args)
        {
            builder.Append(Label)
                .Append(": ")
                .Append(bundle(type))
                .Append(": ")
                .Append(string.Format(bundle(message), args))
                .AppendLine();
        }
    }
}
