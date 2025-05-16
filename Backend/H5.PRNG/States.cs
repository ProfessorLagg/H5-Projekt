using System.Runtime.InteropServices;
using System.Text;

namespace H5.PRNG;
[StructLayout(LayoutKind.Explicit, Size = SizeOfThis)]
public unsafe struct State128 {
	const int SizeOfThis = 128 / 8;
	[FieldOffset(0)]
	public fixed byte u8[SizeOfThis];
	[FieldOffset(0)]
	public fixed ushort u16[SizeOfThis / sizeof(ushort)];
	[FieldOffset(0)]
	public fixed uint u32[SizeOfThis / sizeof(uint)];
	[FieldOffset(0)]
	public fixed ulong u64[SizeOfThis / sizeof(ulong)];

	public State128() {
		for (int i = 0; i < 16; i++) { u8[i] = 0; }
	}
	public State128(params byte[] values) : this() {
		int L = int.Min(values.Length, SizeOfThis);
		for (int i = 0; i < L; i++) {
			u8[i] = values[i];
		}
	}
	public State128(params ushort[] values) : this() {
		int L = int.Min(values.Length, SizeOfThis / sizeof(ushort));
		for (int i = 0; i < L; i++) {
			u16[i] = values[i];
		}
	}
	public State128(params uint[] values) : this() {
		int L = int.Min(values.Length, SizeOfThis / sizeof(uint));
		for (int i = 0; i < L; i++) {
			u32[i] = values[i];
		}
	}
	public State128(params ulong[] values) : this() {
		int L = int.Min(values.Length, SizeOfThis / sizeof(ulong));
		for (int i = 0; i < L; i++) {
			u64[i] = values[i];
		}
	}

	public string ToString8() {
		StringBuilder sb = new();
		for (int i = 0; i < SizeOfThis; i++) {
			sb.Append("0x");
			sb.Append(u8[i].ToString("X"));
			sb.Append(", ");
		}
		return sb.ToString();
	}
	public string ToString16() {
		StringBuilder sb = new();
		for (int i = 0; i < SizeOfThis / sizeof(ushort); i++) {
			sb.Append("0x");
			sb.Append(u16[i].ToString("X"));
			sb.Append(", ");
		}
		return sb.ToString();
	}
	public string ToString32() {
		StringBuilder sb = new();
		for (int i = 0; i < SizeOfThis / sizeof(uint); i++) {
			sb.Append("0x");
			sb.Append(u32[i].ToString("X"));
			sb.Append(", ");
		}
		return sb.ToString();
	}
	public string ToString64() {
		StringBuilder sb = new();
		for (int i = 0; i < SizeOfThis / sizeof(ulong); i++) {
			sb.Append("0x");
			sb.Append(u64[i].ToString("X"));
			sb.Append(", ");
		}
		return sb.ToString();
	}
	public override string ToString() {
		return this.ToString8();
	}

	public static State128 GetSeed() {
		byte[] bytes = new byte[SizeOfThis];
		Random rand = new Random();
		rand.NextBytes(bytes);
		return new State128(bytes);
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