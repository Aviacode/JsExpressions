using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Diagnostics.Contracts;

namespace JsExpressions
{
	/// <summary>
	/// Helps to compose javascript expressions, so we can have more reusable code for manipulating
	/// our client-side VMs in our specflow tests.
	/// </summary>
	public class JsExpression
	{
		private readonly string mExpression;
		private static JsonSerializerSettings sJsonSerializerSettings = new JsonSerializerSettings();

		private static readonly Regex sAlphaCharsRegex = new Regex("^[a-zA-z]*$",
			RegexOptions.Compiled | RegexOptions.CultureInvariant);

		public static JsonSerializerSettings JsonSerializerSettings
		{
			get { return sJsonSerializerSettings; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				sJsonSerializerSettings = value;
			}
		}

		/// <summary>
		/// This is intended to be used by classes that extend <see cref="JsExpression"/> to provide
		/// additional helper methods specific to their situation.
		/// </summary>
		public JsExpression(JsExpression expression)
			: this(expression == null ? null : expression.mExpression)
		{}

		/// <summary>
		/// This is for internal use only. Call the static <see cref="Raw(string)"/> method to create a
		/// <see cref="JsExpression"/> based on a raw C# string.
		/// </summary>
		protected JsExpression(string rawExpression)
		{
			mExpression = rawExpression ?? "null";
		}

		/// <summary>
		/// Creates a <see cref="JsExpression"/> represented by the given C# string.
		/// </summary>
		/// <example><code>
		///     var body = JsExpression.Raw("$('body')");
		/// </code></example>
		[Pure]
		public static JsExpression Raw(string expression)
		{
			return new JsExpression(expression);
		}

		/// <summary>
		/// Creates a <see cref="JsExpression"/> represented by the given C# string.
		/// </summary>
		/// <example><code>
		///     var body = JsExpression.Raw("$('body.{0}.{1]')", arg1, arg2);
		/// </code></example>
		public static JsExpression Raw(string expression, params object[] arguments)
		{
			return new JsExpression(string.Format(expression, arguments));
		}

		/// <summary>
		/// Creates a <see cref="JsExpression"/> representing a Javascript literal string.
		/// </summary>
		/// <example><code>
		///     var message = JsExpression.Literal("hello world");
		///     var alert = JsExpression.Raw("console.log").Call(message); // console.log("hello world")
		/// </code></example>
		[Pure]
		public static StringJsExpression Literal(string value)
		{
			return new StringJsExpression(new JsExpression(JsonConvert.SerializeObject(value, JsonSerializerSettings)));
		}

		/// <summary>
		/// Creates a <see cref="JsExpression"/> representing a Javascript literal number.
		/// </summary>
		/// <example><code>
		///     const long answerToLifeTheUniverseAndEverything = 42;
		///     var message = JsExpression.Literal(answerToLifeTheUniverseAndEverything);
		///     var access = JsExpression.Raw["arr"][message]; // arr[42]
		/// </code></example>
		[Pure]
		public static NumberJsExpression Literal(long value)
		{
			return new NumberJsExpression(new JsExpression(value.ToString()));
		}

		/// <summary>
		/// Creates a <see cref="JsExpression"/> representing a Javascript literal number.
		/// </summary>
		/// <example><code>
		///     const decimal hourlyWage = 15.00m;
		///     var jsExpr = JsExpression.Literal(hourlyWage); // 15.00
		/// </code></example>
		[Pure]
		public static NumberJsExpression Literal(double value)
		{
			return new NumberJsExpression(new JsExpression(value.ToString(CultureInfo.InvariantCulture)));
		}

		/// <summary>
		/// Creates a <see cref="JsExpression"/> representing a Javascript literal boolean.
		/// </summary>
		/// <example><code>
		///     var message = JsExpression.Literal(true);
		///     var alert = JsExpression.Raw("console.log").Call(message); // console.log(true)
		/// </code></example>
		[Pure]
		public static BooleanJsExpression Literal(bool value)
		{
			return new BooleanJsExpression(new JsExpression(value.ToString().ToLower()));
		}

		/// <summary>
		/// Serializes the given value into JavaScript Object Notation (JSON) and uses that as the
		/// basis for a <see cref="JsExpression"/>.
		/// </summary>
		/// <example><code>
		///     var message = JsExpression.Object(new IdNamePair {Id = "Organizations/1", Name = "SNOWCRK" });
		///     var alert = JsExpression.Raw("console.log").Call(message); // console.log({id: "Organizations/1", name: "SNOWCRK"})
		/// </code></example>
		[Pure]
		public static JsExpression Object(object value)
		{
			return new JsExpression(JsonConvert.SerializeObject(value, sJsonSerializerSettings));
		}

		/// <summary>
		/// Creates an expression representing a JavaScript array, containing each of the given elements as expressions.
		/// </summary>
		/// <example><code>
		///     var arr = JsExpression.Array(
		///          JsExpression.Object(new IdNamePair {Id = "Organizations/1", Name = "SNOWCRK" }),
		///          42,              // some simple types like ints and strings are implicitly cast as literal JsExpressions
		///          "hello world");
		///     // [{id: "Organizations/1", name: "SNOWCRK"}, 42, "hello world"];
		/// </code></example>
		[Pure]
		public static JsExpression Array(params JsExpression[] elements)
		{
			return Array(elements.AsEnumerable());
		}

		/// <summary>
		/// Creates an expression representing a JavaScript array, containing each of the given elements as expressions.
		/// </summary>
		/// <example><code>
		///     var arr = JsExpression.Array(Enumerable.Range(1, 3).Select(JsExpression.Literal); // [1, 2, 3]
		/// </code></example>
		[Pure]
		public static JsExpression Array(IEnumerable<JsExpression> elements)
		{
			return new JsExpression("[" + string.Join(", ", elements.Select(e => e.ToString())) + "]");
		}

		/// <summary>
		/// An expression representing a literal `null` in javascript.
		/// </summary>
		/// <example><code>
		///     var alert = JsExpression.Raw("console.log").Call(JsExpression.Null); // console.log(null)
		/// </code></example>
		public static readonly NullJsExpression Null = new NullJsExpression();

		/// <summary>
		/// An expression representing a literal `undefined` in javascript.
		/// </summary>
		/// <example><code>
		///     var alert = JsExpression.Raw("console.log").Call(JsExpression.Undefined); // console.log(undefined)
		/// </code></example>
		public static readonly UndefinedJsExpression Undefined = new UndefinedJsExpression();

		public static implicit operator JsExpression(long value)
		{
			return Literal(value);
		}

		public static implicit operator JsExpression(double value)
		{
			return Literal(value);
		}

		public static implicit operator JsExpression(string value)
		{
			return Literal(value);
		}

		public static implicit operator JsExpression(bool value)
		{
			return Literal(value);
		}

		/// <summary>
		/// Creates a JavaScript object/array access with the given expression as the index value.
		/// </summary>
		/// <example><code>
		///     var firstKey = JsExpression.Raw("arr")[0]; // arr[0]
		///     var firstValue = JsExpression.Raw("dict")[firstKey]; // dict[arr[0]]
		/// </code></example>
		public JsExpression this[JsExpression expression]
		{
			get { return new JsExpression(mExpression + "[" + expression + "]"); }
		}

		/// <summary>
		/// Creates a JavaScript object property access with the given string as the property name.
		/// </summary>
		/// <example><code>
		///     var hello = JsExpression.Raw("dict")["hello"]; // dict.hello
		///     var helloWorld = JsExpression.Raw("dict")["hello world"]; // dict["hello world"]
		/// </code></example>
		public JsExpression this[string str]
		{
			get
			{
				return sAlphaCharsRegex.IsMatch(str)
					? new JsExpression(mExpression + "." + str)
					: this[Literal(str)];
			}
		}

		/// <summary>
		/// Generates a JsExpression representing a strict check that this JsExpression is not undefined.
		/// </summary>
		public BooleanJsExpression IsDefined
		{
			get { return IsStrictlyNotEqualTo(Undefined); }
		}

		/// <summary>
		/// Generates a JsExpression representing a strict check that this JsExpression is undefined.
		/// </summary>
		public BooleanJsExpression IsUndefined
		{
			get { return IsStrictlyEqualTo(Undefined); }
		}

		/// <summary>
		/// Generates a JsExpression representing a strict check that this JsExpression is null.
		/// </summary>
		public BooleanJsExpression IsNull
		{
			get { return IsStrictlyEqualTo(Null); }
		}

		/// <summary>
		/// Generates a JsExpression representing a strict check that this JsExpression is not null.
		/// </summary>
		public BooleanJsExpression IsNotNull
		{
			get { return IsStrictlyNotEqualTo(Null); }
		}

		/// <summary>
		/// Generates a JsExpression representing a strict check that this JsExpression is not undefined and not null.
		/// </summary>
		public BooleanJsExpression IsDefinedAndNotNull
		{
			get { return IsDefined.And(IsNotNull); }
		}

		/// <summary>
		/// Generates a JsExpression representing a strict check that this JsExpression is undefined or null.
		/// </summary>
		public BooleanJsExpression IsUndefinedOrNull
		{
			get { return IsUndefined.Or(IsNull); }
		}

		/// <summary>
		/// Generates a JsExpression to represent a less-than comparison of two other JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length") &lt; 3;</code></example>
		/// </summary>
        public BooleanJsExpression IsLessThan(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " < " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a greater-than comparison of two other JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsGreaterThan(3);</code></example>
		/// </summary>
        public BooleanJsExpression IsGreaterThan(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " > " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a less-than-or-equal comparison of two other JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsLessThanOrEqualTo(3);</code></example>
		/// </summary>
        public BooleanJsExpression IsLessThanOrEqualTo(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " <= " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a greater-than-or-equal comparison of two other JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsGreaterThanOrEqualTo(3);</code></example>
		/// </summary>
        public BooleanJsExpression IsGreaterThanOrEqualTo(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " >= " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a loose equality comparison of two JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsEqualTo(3);</code></example>
		/// </summary>
        public BooleanJsExpression IsEqualTo(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " == " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a loose inequality comparison of two JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsNotEqualTo(3);</code></example>
		/// </summary>
        public BooleanJsExpression IsNotEqualTo(JsExpression other)
		{
			return new BooleanJsExpression(Raw("(" + this + " != " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a strict equality comparison of two JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsStrictlyEqualTo(3);</code></example>
		/// </summary>
		public BooleanJsExpression IsStrictlyEqualTo(JsExpression other)
		{
			return new BooleanJsExpression(Raw("(" + this + " === " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a strict inequality comparison of two JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Raw("$('div').length").IsStrictlyNotEqualTo(3);</code></example>
		/// </summary>
        public BooleanJsExpression IsStrictlyNotEqualTo(JsExpression other)
		{
			return new BooleanJsExpression(Raw("(" + this + " !== " + other + ")"));
		}
	
		/// <summary>
		/// Generates a JsExpression to represent a boolean "or" operation on two JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Literal(true).Or(false);</code></example>
		/// </summary>
        public BooleanJsExpression Or(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " || " + other + ")"));
		}

		/// <summary>
		/// Generates a JsExpression to represent a boolean "and" operation on two JsExpressions.
		/// <example><code>JsExpression condition = JsExpression.Literal(true).And(false);</code></example>
		/// </summary>
		public BooleanJsExpression And(JsExpression other)
		{
            return new BooleanJsExpression(Raw("(" + this + " && " + other + ")"));
		}

		/// <summary>
		/// Creates an expression representing a function call, assuming that this expression represents
		/// a function.
		/// </summary>
		/// <example><code>
		///     var alert = JsExpression.Raw("console.log").Call("hello", 1); // console.log("hello", 1)
		///     var helloWorld = JsExpression.Raw("dict")["hello world"]; // dict["hello world"]
		/// </code></example>
		[Pure]
		public JsExpression Call(params JsExpression[] args)
		{
			args = args ?? new JsExpression[0];
			return new JsExpression(this + "(" + string.Join(", ", args.Select(a => a.ToString())) + ")");
		}

		public T As<T>() where T : JsExpression
		{
			var expressionType = typeof(T);
			var constructor = expressionType.GetConstructor(new[] {typeof(T)});
			var t = (T)constructor.Invoke(new [] {this});

			return t;
		}

		/// <summary>
		/// Creates an <see cref="ArrayJsExpression"/> based on this expression.
		/// </summary>
		[Pure]
		public ArrayJsExpression AsArray()
		{
			return new ArrayJsExpression(this);
		}

		/// <summary>
		/// Creates an <see cref="ArrayJsExpression{T}"/> whose items are expected to be defined by the
		/// given <paramref name="createItem"/> parameter's return value.
		/// </summary>
		[Pure]
		public ArrayJsExpression<T> AsArray<T>(Func<JsExpression, T> createItem )
			where T : JsExpression
		{
			return new ArrayJsExpression<T>(this, createItem);
		}

		/// <summary>
		/// Produces the JavaScript string represented by this <see cref="JsExpression"/>
		/// </summary>
		[Pure]
		public override string ToString()
		{
			return mExpression;
		}
	}
}
