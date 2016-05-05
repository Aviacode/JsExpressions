using System;
using System.Linq;

namespace JsExpressions
{
    public class UndefinedJsExpression : JsExpression
    {
        public UndefinedJsExpression()
            : base("undefined")
        { }
    }
}
