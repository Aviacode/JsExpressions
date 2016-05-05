namespace JsExpressions
{
	/// <summary>
	/// Represents a JQuery object result, which is like an array of elements with some
	/// helper methods to manipulate all of those elements.
	/// </summary>
	/// <remarks>
	/// As you find the need to call jQuery methods that aren't here yet, please add them
	/// to this type.
	/// </remarks>
	public class JQueryJsExpression : ArrayJsExpression<ElementJsExpression>
	{
		public JQueryJsExpression(JsExpression expression) 
			: base(expression, e => new ElementJsExpression(e))
		{}

		/// <summary>
		/// Creates an expression that represents calling "prop(...)" on a jQuery object.
		/// <example><code>var disabled = JQuery.Find(".submit-button").Prop("disabled");</code></example>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public JsExpression Prop(JsExpression propertyName)
		{
			return this["prop"].Call(propertyName);
		}
	}
}