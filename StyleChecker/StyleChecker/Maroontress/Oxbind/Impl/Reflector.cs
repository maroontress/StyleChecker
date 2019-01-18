namespace Maroontress.Oxbind.Impl
{
    using System;

    /// <summary>
    /// Capsulizes each step to realize the injection.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the value to be injected.
    /// </typeparam>
    public sealed class Reflector<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Reflector{T}"/> class.
        /// </summary>
        /// <param name="inject">
        /// The delegate that injects the sugarcoated value to the field or
        /// with the method.
        /// </param>
        /// <param name="unitType">
        /// Get the type of the unit.
        /// </param>
        /// <param name="sugarcoater">
        /// The delegate that sugarcoats the specified value.
        /// </param>
        public Reflector(
            Injector inject, Type unitType, Sugarcoater<T> sugarcoater)
        {
            Inject = inject;
            UnitType = unitType;
            Sugarcoater = sugarcoater;
        }

        /// <summary>
        /// Gets the injector.
        /// </summary>
        public Injector Inject { get; }

        /// <summary>
        /// Gets the type of the unit.
        /// </summary>
        public Type UnitType { get; }

        /// <summary>
        /// Gets the sugarcoater.
        /// </summary>
        public Sugarcoater<T> Sugarcoater { get; }
    }
}
