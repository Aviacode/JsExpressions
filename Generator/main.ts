/// <reference path="typings/main.d.ts" />

/* https://github.com/Microsoft/TypeScript/wiki/Using-the-Compiler-API#using-the-compiler-api */
import * as ts from 'typescript';
import * as path from 'path';
import * as fs from 'fs';
import { JsExpressionFileBuilder } from './jsExpressionFileBuilder';
let glob = require('glob');
let packageJson = require('./package.json');

let  args = process.argv.slice(2);
let  searchPath = args[0];
let  outputPath = args[1];
let  defaultNamespace = args[2];

console.log('Looking for files using "' + searchPath + '"...');
glob(searchPath, null, (err, files: string[]) => {
    if (err) {
        throw err;
    }

    if (files.length === 0) {
        console.log('No files found.');
        return;
    };

    files = files.filter((f) => {
        return !f.endsWith('Specs.ts');
    });

    if (files.length === 0) {
        console.log('No files remaining after removing files that end in "Specs.ts".');
        return;
    };
    console.log('Done!');
    
    console.log('Parsing typescript from ' + files.length + ' file(s)...');
    let compilerOptions: ts.CompilerOptions = {
        target: ts.ScriptTarget.Latest,
        module: ts.ModuleKind.CommonJS,
        noResolve: true
    };

    let program = ts.createProgram(files, compilerOptions);
    console.log('Done!');

    console.log('Extracting metadata...');
    outputPath = path.normalize(outputPath);
    let checker = program.getTypeChecker();
    let jsExpressionFileBuilder = new JsExpressionFileBuilder(checker, defaultNamespace, packageJson.name, packageJson.version);
    let fileNames = files.map((f) => { return path.parse(f).base; });
    
    for (const sourceFile of program.getSourceFiles()) {
        let fileName = path.parse(sourceFile.fileName).base;
        
        if (fileNames.indexOf(fileName) > -1) {
            //console.log(' - ' + fileName);
            ts.forEachChild(sourceFile, processNode);
        }
    }
    console.log('Done!');
    
    console.log(`Writing file to ${outputPath}...`);
    fs.writeFile(outputPath, jsExpressionFileBuilder.build());
    console.log('Done!');

    function processNode(node: ts.Node) {
        let nodeKind = node.kind;
        
        switch(node.kind){
            case ts.SyntaxKind.ClassDeclaration:
            case ts.SyntaxKind.InterfaceDeclaration:
                jsExpressionFileBuilder.withClass(<ts.ClassDeclaration | ts.InterfaceDeclaration>node);    
                break;
            case ts.SyntaxKind.EnumDeclaration:
                jsExpressionFileBuilder.withEnum(<ts.EnumDeclaration>node);
                break;
            case ts.SyntaxKind.ModuleDeclaration:
            case ts.SyntaxKind.ModuleBlock:
                ts.forEachChild(node, processNode);
                break;
        }
    }
});