class sfc32 {
    static getSeed() {
        const buffer = new ArrayBuffer((32 / 8) * 4);
        const view = new Uint32Array(buffer);
        view[0] = (Math.random() * 2 ** 32) >>> 0;
        view[1] = (Math.random() * 2 ** 32) >>> 0;
        view[2] = (Math.random() * 2 ** 32) >>> 0;
        view[3] = (Math.random() * 2 ** 32) >>> 0;
        return view;
    }

    static test() {
        let seed = new Uint32Array(4);
        seed[0] = 9 - 0;
        seed[1] = 9 - 1;
        seed[2] = 9 - 2;
        seed[3] = 9 - 3;
        let result = {
            seed: Array.from(seed),
            integers: [],
        };
        let rand = new sfc32(seed);
        const iterCount = 8;

        for (let i = 0; i < iterCount; i++) {
            const v = rand.nextInt();
            result.integers.push(v);
        }

        return JSON.stringify(result);
    }

    constructor(s) {
        if (!(s instanceof Uint32Array)) { throw new TypeError("seed was not an instance of Uint32Array") }
        if (s.length !== 4) { throw new TypeError(`Expected seed.length = 4, but found: ${s.length}`) }
        this.count = 0;
        this.seed = new Uint32Array(4);
        this.state = new Uint32Array(4);
        for (let i = 0; i < 4; i++) {
            this.seed[i] = s[i];
            this.state[i] = s[i];
        }
    }

    /**
     * 
     * @returns Random integer between 0 and 2^32
     */
    nextInt() {
        let tmp_buf = new ArrayBuffer(4);
        let temp_u32 = new Uint32Array(tmp_buf);
        temp_u32[0] = this.state[0] + this.state[1] + this.state[3]
        this.state[3] = this.state[3] + 1;
        this.state[0] = this.state[1] ^ (this.state[1] >>> 9);
        this.state[1] = this.state[2] + (this.state[2] << 3);
        this.state[2] = (this.state[2] << 21 | this.state[2] >>> 11);
        this.state[2] = this.state[2] + temp_u32[0];
        temp_u32[0] = temp_u32[0] >>> 0;

        this.count += 1;
        return temp_u32[0];
    }

    /**
     * 
     * @returns Random float between 0.0 and 1.0
     */
    nextFloat() {
        return this.nextInt() / 4294967296;
    }
}

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