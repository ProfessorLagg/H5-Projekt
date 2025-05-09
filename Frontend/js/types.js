// === Basic typeof checks ===
function isUndefined(v) {
    return typeof (v) === "undefined";
}
function isBoolean(v) {
    return typeof (v) === "boolean";
}
function isNumber(v) {
    return typeof (v) === "number";
}
function isBigint(v) {
    return typeof (v) === "bigint";
}
function isString(v) {
    return typeof (v) === "string";
}
function isSymbol(v) {
    return typeof (v) === "symbol";
}
function isFunction(v) {
    return typeof (v) === "function";
}
function isObject(v) {
    // typeof(null) === "object". See https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/typeof#typeof_null
    return typeof (v) === "object" && v !== null;
}

// === Number type checks ===
function isInteger(v) {
    return isNumber(v) && Number.isInteger(v);
}
function isFloat(v) {
    return isNumber(v) && Number.isFloat(v);
}

// === Object Type Checks ===
function isArrayBuffer(v) {
    return v instanceof ArrayBuffer;
}
function isArrayBufferView(v) {
    return isObject(v) && ArrayBuffer.isView(v);
}