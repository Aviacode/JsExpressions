import * as ts from 'typescript';
import { JsExpressionClassBuilder } from './jsExpressionClassBuilder';
import { EnumBuilder } from './enumBuilder';

export class JsExpressionFileBuilder {
    private typeChecker: ts.TypeChecker;
    private baseNamespace: string;
    private name: string;
    private version: string;
    private fileContents = `// ReSharper disable InconsistentNaming
// ReSharper disable RedundantNameQualifier
// ReSharper disable PartialTypeWithSinglePart
using JsExpressions;
using System;
using System.CodeDom.Compiler;`;

    constructor(typeChecker: ts.TypeChecker, baseNamespace: string, toolName: string, toolVersion: string) {
        this.typeChecker = typeChecker;
        this.baseNamespace = baseNamespace;
        this.name = toolName;
        this.version = toolVersion;
    }

    withEnum(node: ts.EnumDeclaration) {
        let identifier = node.name;
        let moduleName = this.getModuleNameFromFullyQualifiedName(identifier);

        let enumBuilder = new EnumBuilder(identifier.text);

        node.members.forEach((enumMember) => {
            let enumText = enumMember.name.getText();
            let enumValue = enumMember.initializer ? parseInt(enumMember.initializer.getText()) : null;
            enumBuilder.withNameValue(enumText, enumValue);
        });

        this.appendType(moduleName, enumBuilder.build());
    }

    withClass(node: ts.ClassDeclaration | ts.InterfaceDeclaration): JsExpressionFileBuilder {
        let identifier = node.name;
        let moduleName = this.getModuleNameFromFullyQualifiedName(identifier);
        let type = this.typeChecker.getTypeAtLocation(node);
        let properties = this.typeChecker.getAugmentedPropertiesOfType(type);
        let typeArguments: string[] = [];

        if (type.flags & ts.TypeFlags.Class) {
            let refType = <ts.TypeReference>type;

            // if it has type arguments, it must be a generic class
            if (refType.typeArguments && refType.typeArguments.length > 0) {
                typeArguments = refType.typeArguments.map((t) => { return t.symbol.name; });
            }
        }

        if (type.flags & ts.TypeFlags.Interface) {
            let interfaceType = <ts.InterfaceType>type;

            // if it has type parameters, it must be a generic interface
            if (interfaceType.typeParameters && interfaceType.typeParameters.length > 0) {
                typeArguments = interfaceType.typeParameters.map((t) => { return t.symbol.name; });
            }
        }

        let classBuilder = new JsExpressionClassBuilder(this.typeChecker, identifier.text, typeArguments);

        properties.forEach((property: ts.Symbol) => {
            let declaration = property.valueDeclaration;
            if (!this.isPrivateOrProtected(declaration.flags)) {
                switch(declaration.kind){
                    case ts.SyntaxKind.PropertySignature:
                    case ts.SyntaxKind.GetAccessor:
                    case ts.SyntaxKind.SetAccessor:
                    case ts.SyntaxKind.PropertyDeclaration:
                    case ts.SyntaxKind.Parameter:
                        classBuilder.withProperty(<ts.PropertySignature | ts.PropertyDeclaration | ts.ParameterDeclaration>declaration);
                        break;
                    case ts.SyntaxKind.MethodSignature:
                    case ts.SyntaxKind.MethodDeclaration:
                        classBuilder.withMethod(<ts.MethodSignature | ts.MethodDeclaration>declaration);
                        break;
                    default:
                        console.log(ts.SyntaxKind[declaration.kind]);
                        break;
                }
            }
        });

        this.appendType(moduleName, classBuilder.build());

        return this;
    }

    build(): string {
        return this.fileContents;
    }

    private appendType(moduleName: string, typeDefinition: string){
        this.fileContents += ` 

namespace ${this.baseNamespace}${moduleName ? '.' + moduleName : ''} 
{
    [GeneratedCodeAttribute("${this.name}", "${this.version}")]`;
        this.fileContents += typeDefinition;
        
        this.fileContents += `
}`;
    }

    private getModuleNameFromFullyQualifiedName(id: ts.Identifier): string {
        let symbol = this.typeChecker.getSymbolAtLocation(id);
        let fullyQualifiedName = this.typeChecker.getFullyQualifiedName(symbol);

        if (fullyQualifiedName === id.text) {
            return null;
        }

        return fullyQualifiedName.substr(0, fullyQualifiedName.length - (id.text.length + 1));
    }

    private isPrivateOrProtected(flags: ts.NodeFlags): boolean {
        return ((flags & (ts.NodeFlags.Private | ts.NodeFlags.Protected)) > 0);
    }
}