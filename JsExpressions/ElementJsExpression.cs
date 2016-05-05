namespace JsExpressions
{
	/// <summary>
	/// Represents a <see cref="JsExpression"/> that should yield an HTML element.
	/// </summary>
	public class ElementJsExpression : JsExpression
	{
		public ElementJsExpression(JsExpression expression) : base(expression)
		{}
	}
}