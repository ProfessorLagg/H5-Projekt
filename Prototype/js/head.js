function isBoolean(v) { return typeof v === 'boolean'; }
function isNumber(v) { return typeof v === 'number'; }
function isBigint(v) { return typeof v === 'bigint'; }
function isString(v) { return typeof v === 'string'; }
function isSymbol(v) { return typeof v === 'symbol'; }
function isFunction(v) { return typeof v === 'function'; }
function isObject(v) { return v !== null && v != undefined && typeof v === 'object'; }

function expectEqual(expect, value) {
    const valid = expect === value;
    if (!valid) { console.log(`expected ${expect} but found ${value}`); }
    return valid;
}
