using System;
using System.Linq;

namespace JsExpressions
{
    public class DateJsExpression : JsExpression
    {
        public DateJsExpression(JsExpression expression)
            : base(expression)
        { }

	    public StringJsExpression ToISOString()
	    {
			return new StringJsExpression(this["toISOString"].Call());
	    }

        public static implicit operator DateJsExpression(NullJsExpression nullJsExpression)
        {
            return new DateJsExpression(nullJsExpression);
        }
    }
}
