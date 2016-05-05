import * as ts from 'typescript';

interface IProperty {
    name: string;
    type: ts.Type;
}

interface IParameter extends IProperty {

}

interface IMethod extends IProperty {
    typeParameters: string[];
    parameters: IParameter[];
}

export class JsExpressionClassBuilder {
    private typeChecker: ts.TypeChecker;
    private className: string;
    private typeParameters: string[];
    private properties: IProperty[];
    private methods: IMethod[];

    constructor(typeChecker: ts.TypeChecker, className: string, typeParameters?: string[]) {
        this.typeChecker = typeChecker;

        this.className = className;
        this.typeParameters = typeParameters || [];
        this.properties = [];
        this.methods = [];
    }

    withProperty(node: ts.PropertyDeclaration | ts.PropertySignature | ts.ParameterDeclaration): JsExpressionClassBuilder {
        let symbol = this.typeChecker.getSymbolAtLocation(node.name);
        let propertyName = symbol.name;
        let propertyType = this.typeChecker.getTypeAtLocation(node);
        
        let signatures = propertyType.getCallSignatures();
        
        // This means the property is a function type, so let's actually treat it as a method
        if(signatures.length > 0){
            signatures.forEach((signature) => {
                let methodType = signature.declaration.type ? this.typeChecker.getTypeAtLocation(signature.declaration.type) : null;
                
                let typeParameters: string[] = [];
                if(signature.typeParameters){
                    typeParameters = signature.typeParameters.map((value: ts.TypeParameter) => {
                        return value.symbol.name;
                    });
                }
                let parameters:IParameter[] = [];
                if(signature.parameters){
                    parameters = signature.parameters.map((value: ts.Symbol) => {
                        return {
                            name: value.name,
                            type: this.typeChecker.getTypeOfSymbolAtLocation(value, node)
                        };
                    });
                }
                                
                this.methods.push({
                    name: propertyName,
                    type: methodType,
                    typeParameters: typeParameters,
                    parameters: parameters
                });                
            });            
        }
        else{
            this.properties.push({
                name: propertyName,
                type: propertyType
            });
        }


        return this;
    }

    withMethod(method: ts.MethodDeclaration | ts.MethodSignature): JsExpressionClassBuilder {
        let symbol = this.typeChecker.getSymbolAtLocation(method.name);
        let methodName = symbol ? symbol.name : method.name.getText();
        let signature = this.typeChecker.getSignatureFromDeclaration(method);
        let methodType = this.typeChecker.getReturnTypeOfSignature(signature);

        let typeParameters: string[] = [];

        if (signature.typeParameters) {
            typeParameters = signature.typeParameters.map((value: ts.TypeParameter) => {
                return value.symbol.name;
            });
        }

        let parameters: IParameter[] = [];

        if (method.parameters) {
            parameters = method.parameters.map((value: ts.ParameterDeclaration) => {
                return {
                    name: value.name.getText(),
                    type: this.typeChecker.getTypeAtLocation(value)
                };
            });
        }

        this.methods.push({
            name: methodName,
            type: methodType,
            typeParameters: typeParameters,
            parameters: parameters
        });

        return this;
    }

    build(): string {
        let className = this.sanitizeForCSharpIdentifier(this.className) + 'JsExpression';
        let typeParameters = this.typeParameters.join(', ');

        let classString = `
    public partial class ${className}${typeParameters ? '<' + typeParameters + '>' : ''} : JsExpression ${typeParameters ? `
        where ${typeParameters}: JsExpression` : ''}
    {
        public ${className}(JsExpression expression) : base(expression) { }`;

        this.properties.forEach((p) => {
            classString += this.buildProperty(p);
        });

        this.methods.forEach((m) => {
            classString += this.buildMethod(m);
        });

        classString += `
    }`;

        return classString;
    }

    private buildProperty(property: IProperty): string {
        let propertyName = this.sanitizeForCSharpIdentifier(property.name);
        let propertyType = this.getReturnTypeString(property.type);
        let propertyBody = this.getReturnExpression(property.type, property.name);

        return `

        public ${propertyType} ${propertyName} 
        {
            get 
            {
                return ${propertyBody};
            }
        }`;
    }

    private buildMethod(method: IMethod): string {
        let methodName = method.name;
        let sanitizedMethodName = this.sanitizeForCSharpIdentifier(method.name);
        let parameters = method.parameters.map((p) => { return {name: this.sanitizeForCSharpIdentifier(p.name, true), type: this.getReturnTypeString(p.type)}; })
        let parameterNames = parameters.map((p) => p.name);
        let typeParameters = method.typeParameters.join(', ');
        let methodType = this.getReturnTypeString(method.type);
        let call = `this["${methodName}"].Call(${parameterNames.join(', ')})`;
        let methodBody = methodType === 'JsExpression' ? call : this.getReturnExpression(method.type, methodName, call);
        let newKeyword = sanitizedMethodName === '@ToString' && parameters.length === 0 ? 'new ' : '';
        
        return `

        public ${newKeyword}${methodType} ${sanitizedMethodName}${typeParameters ? '<' + typeParameters + '>' : ''}(${parameters.map(p => `${p.type} ${p.name}`).join(', ')})
        {
            return ${methodBody};
        }`;
    }

    private getReturnTypeString(type: ts.Type): string {
        if(type === null) {
            return 'JsExpression'
        }
        
        if (type.symbol) {
            if (this.typeParameters.indexOf(type.symbol.name) > -1) {
                return type.symbol.name;
            }

            // TODO: Remove this after our VMs use instant and localdate
            if (type.symbol.name === 'Date') {
                return 'DateJsExpression';
            }
            
            if (type.symbol.name === 'Function') {
                return 'JsExpression';
            }
            
            if(type.symbol.name === 'KeyboardEvent') {
                return 'JsExpression';
            }

            if (type.flags & (ts.TypeFlags.Reference | ts.TypeFlags.Interface)) {
                let refType = <ts.TypeReference>type;
                let typeString = this.typeChecker.getFullyQualifiedName(type.symbol) + 'JsExpression';

                if (refType.typeArguments) {
                    let innerTypes = refType.typeArguments.map((t) => { return this.getReturnTypeString(t); });
                    let typeArguments = (typeString === 'ArrayJsExpression' && innerTypes[0] === 'JsExpression') ? '' : '<' + innerTypes.join(', ') + '>';

                    return typeString + typeArguments;
                }

                return typeString;
            }
        }

        if(type.flags & ts.TypeFlags.NumberLike)
            return 'NumberJsExpression';

        if(type.flags & ts.TypeFlags.String)
            return 'StringJsExpression';
                   
        if(type.flags & ts.TypeFlags.Boolean){
            return 'BooleanJsExpression';
        }        

        return 'JsExpression';
    }

    private getReturnExpression(type: ts.Type, propertyName: string, expressionArgument?: string, depth?: number): string {
        depth = depth || 0;
        let propertyNameExpression = `this["${propertyName}"]`;
        expressionArgument = expressionArgument || propertyNameExpression;
        
        if(type === null){
            return expressionArgument;
        }
        
        if (type.symbol) {
            if (this.typeParameters.indexOf(type.symbol.name) > -1) {
                return `(${type.symbol.name})Activator.CreateInstance(typeof(${type.symbol.name}), ${expressionArgument})`;
            }

            // TODO: Remove this after our VMs use instant and localdate
            if (type.symbol.name === 'Date') {
                return `new DateJsExpression(${expressionArgument})`;
            }
            
            if (type.symbol.name === 'Function') {
                return expressionArgument;
            }
            
            if( type.symbol.name === 'KeyboardEvent'){
                return expressionArgument;
            }

            if (type.flags & (ts.TypeFlags.Reference | ts.TypeFlags.Interface)) {
                let refType = <ts.TypeReference>type;
                let typeString = this.typeChecker.getFullyQualifiedName(type.symbol) + 'JsExpression';

                if (refType.typeArguments) {
                    let innerTypes = refType.typeArguments.map((t) => { return this.getReturnTypeString(t); });
                    let isArray = typeString === 'ArrayJsExpression';
                    let isGenericArray = isArray && innerTypes[0] !== 'JsExpression';
                    let typeArguments = isGenericArray || (!isArray && innerTypes) ? '<' + innerTypes.join(', ') + '>' : '';
                    
                    let lambaVariableName = `e${depth}`;
                    let secondArgument =  isGenericArray ? `, ${lambaVariableName} => ${this.getReturnExpression(refType.typeArguments[0], propertyName, lambaVariableName, ++depth)}`: '';

                    return `new ${typeString}${typeArguments}(${expressionArgument}${secondArgument})`;
                }

                return `new ${typeString}(${expressionArgument})`;
            }
        }
        
        if(type.flags & ts.TypeFlags.NumberLike)
            return `new NumberJsExpression(${expressionArgument})`;

        if(type.flags & ts.TypeFlags.String)
            return `new StringJsExpression(${expressionArgument})`;

        if(type.flags & ts.TypeFlags.Boolean)
            return `new BooleanJsExpression(${expressionArgument})`;

        return expressionArgument;
    }

    private sanitizeForCSharpIdentifier(name: string, forMethodParameter?: boolean): string {
        let originalName = name;
        let firstCharacter = name.charAt(0);

        // Ensure first character is a letter or '_' 
        // If the upper case and lower case characters are equal, then it is likely the character was not a letter
        // http://stackoverflow.com/questions/10707972/detecting-if-a-character-is-a-letter 
        while (firstCharacter.toUpperCase() === firstCharacter.toLowerCase() && name[0] != '_') {
            name = name.substring(1);
            firstCharacter = name.charAt(0);
        }

        // Remove any characters that are not a letter, a number, or an underscore
        let validCharacters: string[] = [];
        
        name.split('').forEach(character => {
            if(!Number.isNaN(parseFloat(character)) || character.toUpperCase() !== character.toLowerCase() || character === '_'){
                validCharacters.push(character);
            }
        });

        name = validCharacters.join('');

        if (!forMethodParameter) {
            name = name[0].toUpperCase() + name.substring(1);
        }

        return name = '@' + name;
    }
}