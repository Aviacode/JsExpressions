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

Create expression to represent invoking a function.

```csharp
JsExpression.Raw("$('body').scope()")["viewModal"]["table"]["refresh"].Call();
```

```javascript
$('body').scope().viewModals.table.refresh();
```

##Using Subclasses

This is perhaps the most powerful feature of JsExpressions. When used along side the included JsExpression Generator, we can improve the maintainability of our webdriver based end to end tests by causing compile time errors when TypeScript classes have had changes made that are not reflected in the end to end tests.

Without JsExpression Subclasses
```javascript
JsExpression.Raw("$('body').scope()")["viewModal"]["table"]["rows"];
```

With JsExpression Subclasses
```csharp
viewModal.table.rows
```

##JsExpressionGenerator

Using the generator, you can create CSharp subclasses of parent type JsExpression based on your TypeScript classes. 
```node JsExpressionGenerator [searchPath] [outputPath] [defaultNamespace]```

###Arguments

1. searchPath: The path to search for TypeScript classes, supports Glob search patterns.
2. outputPath: The path where the newly generated CSharp file will be created. Path should include the desired file name.
3. defaultNamespace: This is the desired namespace to use for all generated JsExpression subclasses.

###Setup

1. install node
2. run node JsExpressionGenerator

##Additional benefits

- Makes compile time failures from changes made to TypeScript objects possible.

## Contributors

We've included a subset of support that satisfies our needs but we welcome pull requests.

## License

MIT: http://rem.mit-license.org
