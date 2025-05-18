class TypeChecker {
    // === Basic typeof checks ===
    static isUndefined(v) { return typeof (v) === "undefined"; }
    static isBoolean(v) { return typeof (v) === "boolean"; }
    static isNumber(v) { return typeof (v) === "number"; }
    static isBigint(v) { return typeof (v) === "bigint"; }
    static isString(v) { return typeof (v) === "string"; }
    static isSymbol(v) { return typeof (v) === "symbol"; }
    static isFunction(v) { return typeof (v) === "function"; }
    static isObject(v) {
        // typeof(null) === "object". See https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/typeof#typeof_null
        return typeof (v) === "object" && v !== null;
    }

    // === Additional Null Checks ===
    static isNull(v) { return v === null; }
    static isNullOrUndefined(v) { return TypeChecker.isUndefined(v) || TypeChecker.isNull(v); }

    // === Number type checks ===
    static isInteger(v) { return TypeChecker.isNumber(v) && Number.isInteger(v); }
    static isFloat(v) { return TypeChecker.isNumber(v) && Number.isFloat(v); }
    static isFinite(v) { return TypeChecker.isNumber(v) && Number.isFinite(v); }

    // === Object Type Checks ===
    static isArrayBuffer(v) { return v instanceof ArrayBuffer; }
    static isArrayBufferView(v) { return TypeChecker.isObject(v) && ArrayBuffer.isView(v); }

    constructor() {
        throw new Error(TypeChecker.name, " is a static class");
    }
}