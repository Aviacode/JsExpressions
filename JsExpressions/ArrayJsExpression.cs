using System;

namespace JsExpressions
{
	/// <summary>
	/// A <see cref="JsExpression"/> representing a JavaScript array.
	/// </summary>
	public class ArrayJsExpression : JsExpression
	{
		public ArrayJsExpression(JsExpression expression)
			: base(expression)
		{ }

		/// <summary>
		/// Gets a <see cref="JsExpression"/> representing this array's "length" property
		/// </summary>
		public NumberJsExpression Length
		{
			get { return new NumberJsExpression(this["length"]); }
		}

		public BooleanJsExpression IsEmpty
		{
			get { return Length.IsStrictlyEqualTo(0); }
		}

		/// <summary>
		/// Produces an expression which, when executed, will remove all values from this array.
		/// </summary>
		public JsExpression Clear()
		{
			return this["splice"].Call(0, Length);
		}

		public ArrayJsExpression Map(string field)
		{
			return new ArrayJsExpression(this["map"].Call(Raw(string.Format("function(i) {{ return i.{0};}}", field))));
		}

		public JsExpression Push(JsExpression expression)
		{
			return this["push"].Call(expression);
		}

		public NumberJsExpression IndexOf(JsExpression expression)
		{
			return new NumberJsExpression(this["indexOf"].Call(expression));
		}
	}

	public class ArrayJsExpression<T> : ArrayJsExpression
		where T : JsExpression
	{
		/// <summary>
		/// The function that can convert an ordinary JsExpression into a JsExpression of type <see cref="T"/>.
		/// </summary>
		public Func<JsExpression, T> CreateItem { get; private set; }

		public ArrayJsExpression(JsExpression expression, Func<JsExpression, T> createItem)
			: base(expression)
		{
			CreateItem = createItem;
		}

		public new T this[JsExpression index]
		{
			get { return CreateItem(base[index]); }
		}
	}
}