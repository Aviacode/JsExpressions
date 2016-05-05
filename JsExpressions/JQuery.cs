using System;

namespace JsExpressions
{
	/// <summary>
	/// A class with methods to represent calls that can be made directly on the jQuery variable ("$").
	/// For methods that can be called on a "jQuery object" ("$('...')"), see <see cref="JQueryJsExpression"/>.
	/// </summary>
	public static class JQuery
	{
		/// <summary>
		/// Produces a <see cref="JQueryJsExpression"/> equivalent to having called $("...").
		/// <example><code>var submitButton = JQuery.Find(".submit-button");</code></example>
		/// </summary>
		/// <param name="selector">
		/// A selector specifying which elements to query for. Usually this will be a literal string, like ".submit-button".
		/// </param>
		public static JQueryJsExpression Find(JsExpression selector)
		{
			return new JQueryJsExpression(JsExpression.Raw("$").Call(selector));
		}
	}
}