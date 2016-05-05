using System;
using System.Linq;
using Newtonsoft.Json;

namespace JsExpressions
{
    public class StringJsExpression : JsExpression
    {
        public StringJsExpression(JsExpression expression)
            : base(expression)
        { }

        public static implicit operator StringJsExpression(string value)
        {
            return Literal(value);
        }

        public static implicit operator StringJsExpression(NullJsExpression nullJsExpression)
        {
            return new StringJsExpression(nullJsExpression);
        }
    }
}
