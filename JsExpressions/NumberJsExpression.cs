using System;
using System.Globalization;
using System.Linq;

namespace JsExpressions
{
    public class NumberJsExpression : JsExpression
    {
        public NumberJsExpression(JsExpression expression)
            : base(expression)
        { }

        public static implicit operator NumberJsExpression(int value)
        {
            return Literal(value);
        }

        public static implicit operator NumberJsExpression(long value)
        {
            return Literal(value);
        }

        public static implicit operator NumberJsExpression(double value)
        {
            return Literal(value);
        }

        public static implicit operator NumberJsExpression(NullJsExpression nullJsExpression)
        {
            return new NumberJsExpression(nullJsExpression);
        }

		public static NumberJsExpression operator -(NumberJsExpression n1, NumberJsExpression n2)
	    {
		    return new NumberJsExpression(Raw("({0} - {1})", n1, n2));
	    }

		public static NumberJsExpression operator +(NumberJsExpression n1, NumberJsExpression n2)
		{
			return new NumberJsExpression(Raw("({0} + {1})", n1, n2));
		}
    }
}
