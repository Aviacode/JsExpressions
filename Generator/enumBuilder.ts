export class EnumBuilder {
    private name: string;
    private nameValuePairs: { name: string, value: number }[] = []

    constructor(name: string) {
        this.name = name;
    }

    withNameValue(name: string, value: number) {
        this.nameValuePairs.push({
            name: name,
            value: value
        });
    }

    build(): string {
        let enumString = `
    public enum ${this.name}
    {`;
        this.nameValuePairs.forEach((pair) => {
            enumString += `
        ${pair.name}${(pair.value !== null && pair.value !== undefined) ? (' = ' + pair.value) : ''},`;
        });

        if(enumString.endsWith(',')){
            enumString = enumString.substring(0, enumString.length - 1);
        }

        enumString += `
    }`;

        return enumString;
    }
}