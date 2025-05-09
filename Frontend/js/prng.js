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

    constructor(seed) {
        if (!(seed instanceof Uint32Array)) { throw new TypeError("seed was not an instance of Uint32Array") }
        if (seed.length !== 4) { throw new TypeError(`Expected seed.length = 4, but found: ${seed.length}`) }

        this.seed = seed;
        this.count = 0;
        this.state = this.seed;
    }

    /**
     * 
     * @returns Random integer between 0 and 2^32
     */
    int() {
        let t = (this.state[0] + this.state[1]) + this.state[3];
        this.state[3] = this.state[3] + 1;
        this.state[0] = this.state[1] ^ (this.state[1] >>> 9);
        this.state[1] = this.state[2] + (this.state[2] << 3);
        this.state[2] = (this.state[2] << 21 | this.state[2] >>> 11);
        this.state[2] = this.state[2] + t;
        const r = (t >>> 0);

        this.count += 1;
        return r;
    }

    /**
     * 
     * @returns Random float between 0.0 and 1.0
     */
    float() {
        return this.int() / 4294967296;
    }
}
