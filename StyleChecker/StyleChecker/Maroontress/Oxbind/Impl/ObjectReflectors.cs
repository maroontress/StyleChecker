namespace Maroontress.Oxbind.Impl
{
    using System;

    /// <summary>
    /// Provides methods for <see cref="Reflector{T}"/> (<c>T</c> is
    /// <c>object</c>).
    /// </summary>
    public static class ObjectReflectors
    {
        private static readonly Sugarcoater<object> Eventizer
            = Readers.NewEventObject;

        private static readonly Sugarcoater<object> PassThrough
            = (r, v) => v;

        /// <summary>
        /// Creates a new <see cref="Reflector{T}"/> (<c>T</c> is
        /// <c>object</c>) and perform the specified action
        /// with the placeholder type and the new reflector
        /// associated with the specified type and injector.
        /// </summary>
        /// <param name="type">
        /// The type of the value to be injected.
        /// </param>
        /// <param name="injector">
        /// The injector that injects the value to the field or with the
        /// method.
        /// </param>
        /// <param name="action">
        /// The action that consumes two parameters,
        /// the one is the placeholder type,
        /// the other is the reflector object.
        /// </param>
        public static void Of(
            Type type,
            Injector injector,
            Action<Type, Reflector<object>> action)
        {
            Reflector<T> Of<T>(Type unitType, Sugarcoater<T> sugarcoater)
                => new Reflector<T>(injector, unitType, sugarcoater);

            TripletAction(type, (p, u, s) => action(p, Of(u, s)));
        }

        private static void TripletAction(
            Type type, Action<Type, Type, Sugarcoater<object>> action)
        {
            void Dispatch(Type u, Func<Type, Type> p)
            {
                if (!Types.IsRawType(u, Types.BindEventT))
                {
                    action(type, u, PassThrough);
                    return;
                }
                action(p(Types.FirstInnerType(u)), u, Eventizer);
            }

            Type ToGenericType(Type u)
                => Types.IEnumerableT.MakeGenericType(u);

            if (!Types.IsRawType(type, Types.IEnumerableT))
            {
                Dispatch(type, u => u);
                return;
            }
            Dispatch(Types.FirstInnerType(type), ToGenericType);
        }
    }
}
/*
    | Type of value               | PlaceholderType  | UnitType       | Sugarcoater  |
    | :---                        | :---             | :---           | :---         |
    | `T`                         | `T`              | `T`            | Pass through |
    | `BindEvent<T>`              | `T`              | `BindEevnt<T>` | Eventizer    |
    | `IEnumerable<T>`            | `IEnumerable<T>` | `T`            | Pass through |
    | `IEnumerable<BindEvent<T>>` | `IEnumerable<T>` | `BindEvent<T>` | Eventizer    |
*/
