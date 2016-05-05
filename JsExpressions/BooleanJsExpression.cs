using System;
using System.Linq;

namespace JsExpressions
{
    public class BooleanJsExpression : JsExpression
    {
        public BooleanJsExpression(JsExpression expression)
            : base(expression)
        { }

        /// <summary>
        /// Generates a JsExpression representing a strict check that this BooleanJsExpression is true.
        /// </summary>
        public BooleanJsExpression IsTrue
        {
            get { return IsStrictlyEqualTo(true); }
        }

        /// <summary>
        /// Generates a JsExpression representing a strict check that this BooleanJsExpression is false.
        /// </summary>
        public BooleanJsExpression IsFalse
        {
            get { return IsStrictlyEqualTo(false); }
        }

        public static implicit operator BooleanJsExpression(bool value)
        {
            return Literal(value);
        }

        public static implicit operator BooleanJsExpression(NullJsExpression nullJsExpression)
        {
            return new BooleanJsExpression(nullJsExpression);
        }
    }
}
