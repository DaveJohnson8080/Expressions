using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MiscUtil.Reflection
{
    /// <summary>
    /// Generic class which copies to its target type from a source
    /// type specified in the Copy method. The types are specified
    /// separately to take advantage of type inference on generic
    /// method arguments.
    /// </summary>
    public static class ProperyCompare
    {
        /// <summary>
        /// Copies all readable properties from the source to a new instance
        /// of TTarget.
        /// </summary>
        public static bool Compare<TLeft, TRight>(TLeft left, TRight right)
        {
            return Comparer<TLeft, TRight>.InstancesEqual(left, right);
        }

        /// <summary>
        /// Static class to efficiently store the compiled delegate which can
        /// do the copying. We need a bit of work to ensure that exceptions are
        /// appropriately propagated, as the exception is generated at type initialization
        /// time, but we wish it to be thrown as an ArgumentException.
        /// </summary>
        private static class Comparer<TLeft, TRight>
        {
            private static Func<TLeft, TRight, bool> comparer;
            private static readonly Exception initializationException;

            internal static bool InstancesEqual(TLeft left, TRight right)
            {
                if (initializationException != null)
                {
                    throw initializationException;
                }
                if (left == null || right == null)
                {
                    throw new ArgumentNullException("Missing operand.`");
                }
                if (typeof (TLeft) != typeof (TRight))
                {
                    throw new ArgumentException("Incompatible types.");
                }

                return comparer(left, right);
            }

            static Comparer()
            {
                try
                {
                    BuildComparer();
                    initializationException = null;
                }
                catch (Exception e)
                {
                    comparer = null;
                    initializationException = e;
                }
            }

            private static void BuildComparer()
            {
                ParameterExpression leftParameter = Expression.Parameter(typeof(TLeft), "left");
                ParameterExpression rightParameter = Expression.Parameter(typeof(TRight), "right");
                var bindings = new List<MemberBinding>();
                var props = from prop in typeof (TLeft).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            where prop.CanRead
                            select prop;

                Expression combined = null;
                foreach (PropertyInfo prop in props)
                {
                    var thisPropEqual = Expression.Equal(Expression.Property(leftParameter, prop),
                        Expression.Property(rightParameter, prop));

                    if (combined == null)
                    {
                        combined = thisPropEqual;
                    }
                    else
                    {
                        combined = Expression.AndAlso(combined, thisPropEqual);
                    }
                }

                if (combined == null)
                {
                    comparer = delegate { return true; };
                }
                else
                {
                    comparer = Expression.Lambda<Func<TLeft, TRight, bool>>(
                        combined, leftParameter, rightParameter).Compile();
                }
            }
        }
    }
}
