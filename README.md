## Synopsis

JavaScript strings can be a pain to manage in C#. JsExpressions provides a class library that allow you to create immutable C# classes that represent JavaScript expressions. The ToString() method has been overriden to return the wrapped JavaScript string. The Generator can be useful in creating the JsExpression classes for you out of your TypeScript files.

## Installation

1. install nuget package

## Notable Methods

###JsExpression.Literal

int, long, decimal, bool, string
```csharp
JsExpression boolExpression = JsExpression.Literal(true);
```

Used by implicit cast operator
```csharp
JsExpression boolExpression = false;
```

###JsExpression.Call

Create an expression to represent invoking a function.

```csharp
JsExpression.Raw("$('body').scope()")["viewModal"]["table"]["refresh"].Call();
```
would create ```javascript $('body').scope().viewModals.table.refresh();```

##Using Subclasses

This is perhaps the most powerful feature of JsExpressions. When used along side the included JsExpression Generator, we can improve the maintainability of our webdriver based end to end tests by causing compile time errors when TypeScript classes have had changes made that are not reflected in the end to end tests.

Without Subclasses
```csharp
JsExpression.Raw("$('body').scope()")["viewModal"]["table"]["rows"];
```

With Subclasses
```csharp
ViewModal.Table.Rows
```

##Additional Examples

###Example-1
It may be useful to think of JsExpression like IQueryable<>--until you pass in userSessionData, you’re doing nothing but building up an immutable “query string” to send to WebDriver. If you only need a single value from an object, there’s no reason to make WebDriver serialize the whole object—put your “select” first, and make the evaluation of your query be the last thing you do. 
GetObjectValue() can be useful when you want several values from the object, but you should typically only use it on smaller objects.  Be sure to avoid objects that might include circular references. 

Instead of:
```csharp
var result = row.GetObjectValue(userSessionData);
return result["shortDescription"].ToString();
```
Try:
```csharp
var shortDescription = row["shortDescription"];
return shortDescription.GetStringValue(userSessionData);
```

###Example-2
It’s tempting to build a big Raw JsExpression and pass values into it using string.Format() syntax. Typically you can accomplish the same thing using other JsExpression techniques. 

Instead of:
```csharp
var row = Raw("_.findWhere({0}, {1})", Table().Rows(), Object(new {cell = fCell}));
```
Try:
```csharp
var dxRow = Raw("_.findWhere").Call(Table().Rows(), Object(new {cell = fCell}));
```

###Example-3
In the example we’ve been using, Table().Rows() returns a RowsJsExpression. It seems like getting the row with a given Cell is probably something we’ll want to do in more than one place, so we can move that part of the method into the RowsJsExpression class. 

Instead of:
```csharp
public void ThenNoShortDescriptionAppears(string cell)
{
	 Area.CurrentArea().GetDescriptionForCell(mSessionData, code).Should().BeNullOrEmpty();
}
```
Try:
```csharp
public void ThenNoShortDescriptionAppears(string cell)
{
	 var row = Area.CurrentArea().Rows().GetRowWithCell(cell);
	 row["shortDescription"].GetStringValue(mSessionData).Should().BeNullOrEmpty();
}
```

###Example-4
If you notice that you’re using a common expression a lot in a given Steps file, feel free to make a reusable shortcut:

Instead of:
```csharp
public void WhenTheUserEnters(string cell)
{
	 Area.CurrentArea().SubArea().AddCell(mSessionData, cell);
	 Area.CurrentArea().SubArea().DoSomethingWithCell(mSessionData, "1", "1");
}
```
Try:
```csharp
public void WhenTheUserEnters(string code)
{
	SubArea.AddProcedureCode(mSessionData, code);
	SubArea.SetLinkProcedureCode(mSessionData, "1", "1");
}

private static SubAreaJsExpression SubArea
{
	 get { return Area.CurrentArea().SubArea(); }
}
```

##JsExpressionGenerator

Using the generator, you can create CSharp subclasses of parent type JsExpression based on your TypeScript classes. 
```node JsExpressionGenerator [searchPath] [outputPath] [defaultNamespace]```

###Arguments

1. searchPath: The path to search for TypeScript classes, supports Glob search patterns.
2. outputPath: The path where the newly generated CSharp file will be created. Path should include the desired file name.
3. defaultNamespace: This is the desired namespace to use for all generated JsExpression subclasses.

###Setup

1. run ```install node```
2. navigate to ~\.bin\Generator\
3. run ```npm install --production```
4. run node JsExpressionGenerator

##Additional benefits

- Makes compile time failures from changes made to TypeScript objects possible.

## Contributors

We've included a subset of support that satisfies our needs but we welcome pull requests.

## License

MIT: http://rem.mit-license.org
