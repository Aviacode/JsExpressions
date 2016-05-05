using System;

namespace JsExpressions
{
	public static class Lodash
	{
		private static readonly JsExpression _ = JsExpression.Raw("_");

		public static JsExpression Find(ArrayJsExpression array, JsExpression criteria)
		{
			return Find(array.AsArray(e => e), criteria);
		}

		public static T Find<T>(ArrayJsExpression<T> array, JsExpression criteria) where T : JsExpression
		{
			return array.CreateItem(_["find"].Call(array, criteria));
		}

		public static JsExpression FindWhere(ArrayJsExpression array, JsExpression properties)
		{
			return Find(array.AsArray(e => e), properties);
		}

		public static T FindWhere<T>(ArrayJsExpression<T> array, JsExpression properties) where T : JsExpression
		{
			return array.CreateItem(_["findWhere"].Call(array, properties));
		}

		public static NumberJsExpression FindIndex(ArrayJsExpression array, JsExpression properties)
		{
			return new NumberJsExpression(_["findIndex"].Call(array, properties));
		}

		public static BooleanJsExpression Any<T>(ArrayJsExpression<T> array, JsExpression criteria) where T : JsExpression
		{
			return _["any"].Call(array, criteria).As<BooleanJsExpression>();
		}

		public static ArrayJsExpression<TResult> Map<TResult>(ArrayJsExpression array, JsExpression selector, Func<JsExpression, TResult> createItem)
			where TResult : JsExpression
		{
			return Map(array, selector).AsArray(createItem);
		}
	
		public static ArrayJsExpression Map(ArrayJsExpression array, JsExpression selector)
		{
			return _["map"].Call(array, selector).AsArray();
		}

		public static NumberJsExpression FindIndex<T>(ArrayJsExpression<T> array, JsExpression criteria) where T : JsExpression
		{
			return new NumberJsExpression(_["findIndex"].Call(array, criteria));
		}

		public static ArrayJsExpression<T> Where<T>(ArrayJsExpression<T> array, JsExpression props) where T : JsExpression
		{
			return _["where"].Call(array, props).AsArray(array.CreateItem);
		}

		public static ArrayJsExpression<T> Reject<T>(ArrayJsExpression<T> array, JsExpression props) where T : JsExpression
		{
			return _["reject"].Call(array, props).AsArray(array.CreateItem);
		}

		public static T FindLast<T>(ArrayJsExpression<T> array, JsExpression props) where T : JsExpression
		{
			return array.CreateItem(_["findLast"].Call(array, props));
		}
	}
}
