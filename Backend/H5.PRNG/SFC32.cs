namespace H5.PRNG;
public unsafe class SFC32 {
    public int Count { get; private set; }
    private readonly State128 Seed;
    private State128 State;

    public SFC32(State128 seed) {
        this.Count = 0;
        this.Seed = seed;
        this.State = seed;
    }
    public SFC32() : this(State128.GetSeed()) { }
    public SFC32(UInt128 seed) {
        this.Count = 0;
        this.Seed = new State128(BitConverter.GetBytes(seed));
        this.State = this.Seed;
    }
    /// <returns>Random integer between 0 and 2^32</returns>
    public uint NextInt() {
        uint t = (State.u32[0] + State.u32[1]) + State.u32[3];
        State.u32[3] = State.u32[3] + 1;
        State.u32[0] = State.u32[1] ^ (State.u32[1] >>> 9);
        State.u32[1] = State.u32[2] + (State.u32[2] << 3);
        State.u32[2] = State.u32[2] << 21 | State.u32[2] >>> 11;
        State.u32[2] = State.u32[2] + t;
        this.Count += 1;
        return t >>> 0;
    }

    /// <returns>Random float between 0.0 and 1.0</returns>
    public float NextFloat() {
        const float max_val_u32_f = uint.MaxValue;
        return Convert.ToSingle(this.NextInt()) / max_val_u32_f;
    }
}
