using System.Runtime.InteropServices;

namespace H5.PRNG;
[StructLayout(LayoutKind.Explicit, Size = SizeOfThis)]
public unsafe struct State128 {
    const int SizeOfThis = 16;
    [FieldOffset(0)]
    public fixed byte u8[SizeOfThis];
    [FieldOffset(0)]
    public fixed UInt16 u16[SizeOfThis];
    [FieldOffset(0)]
    public fixed UInt32 u32[SizeOfThis];
    [FieldOffset(0)]
    public fixed UInt64 u64[SizeOfThis];

    public State128() {
        for (int i = 0; i < 16; i++) { u8[i] = 0; }
    }

    public static State128 GetSeed() {
        State128 result = new State128();
        byte[] bytes = new byte[SizeOfThis];
        Random rand = new Random();
        rand.NextBytes(bytes);
        for (int i = 0; i < SizeOfThis; i++) { result.u8[i] = bytes[i]; }
        return result;
    }
}

[StructLayout(LayoutKind.Explicit, Size = SizeOfThis)]
public unsafe struct State256 {
    const int SizeOfThis = 32;
    [FieldOffset(0)]
    public fixed byte Array8[SizeOfThis];
    [FieldOffset(0)]
    public fixed UInt16 Array16[SizeOfThis / 2];
    [FieldOffset(0)]
    public fixed UInt32 Array32[SizeOfThis / 4];
    [FieldOffset(0)]
    public fixed UInt64 Array64[SizeOfThis / 8];

    public State256() {
        for (int i = 0; i < 16; i++) { Array8[i] = 0; }
    }

    public static State128 GetSeed() {
        State128 result = new State128();
        byte[] bytes = new byte[SizeOfThis];
        Random rand = new Random();
        rand.NextBytes(bytes);
        for (int i = 0; i < SizeOfThis; i++) { result.u8[i] = bytes[i]; }
        return result;
    }
}