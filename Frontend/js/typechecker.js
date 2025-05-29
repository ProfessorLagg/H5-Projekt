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

    // === Assertions ===
    static assertIsUndefined(v) { if (!TypeChecker.isUndefined(v)) { throw Error("Expected undefined, but found " + v) } }
    static assertIsBoolean(v) { if (!TypeChecker.isBoolean(v)) { throw Error("Expected boolean, but found " + v) } }
    static assertIsNumber(v) { if (!TypeChecker.isNumber(v)) { throw Error("Expected number, but found " + v) } }
    static assertIsBigint(v) { if (!TypeChecker.isBigint(v)) { throw Error("Expected bigint, but found " + v) } }
    static assertIsString(v) { if (!TypeChecker.isString(v)) { throw Error("Expected string, but found " + v) } }
    static assertIsSymbol(v) { if (!TypeChecker.isSymbol(v)) { throw Error("Expected symbol, but found " + v) } }
    static assertIsFunction(v) { if (!TypeChecker.isFunction(v)) { throw Error("Expected function, but found " + v) } }

    static assertIsNull(v) { if (!TypeChecker.isInteger(v)) { throw Error("Expected null, but found " + v) } }
    static assertIsNullOrUndefined(v) { if (!TypeChecker.isInteger(v)) { throw Error("Expected null or undefined, but found " + v) } }

    static assertIsInteger(v) { if (!TypeChecker.isInteger(v)) { throw Error("Expected integer, but found " + v) } }
    static assertIsFloat(v) { if (!TypeChecker.isInteger(v)) { throw Error("Expected float, but found " + v) } }
    static assertIsFinite(v) { if (!TypeChecker.isInteger(v)) { throw Error("Expected finite number, but found " + v) } }
}