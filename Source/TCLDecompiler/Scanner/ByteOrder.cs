using System;
using System.IO;

namespace TCLDecompiler {
	public enum ByteOrder {
		Native = 0,
		BigEndian,
		LittleEndian,
	}

	public static class Endian {
		public static readonly ByteOrder Current = BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
		public static readonly bool IsBigEndian = Current == ByteOrder.BigEndian;
		public static readonly bool IsLittleEndian = Current == ByteOrder.LittleEndian;

		public static short EndianReverse(this short value) {
			unchecked {
				return (short)(
					(((value >> 00) & 0xFF) << 08) |
					(((value >> 08) & 0xFF) << 00)
				);
			}
		}

		public static int EndianReverse(this int value) =>
			(((value >> 24) & 0xFF) << 00) |
			(((value >> 16) & 0xFF) << 08) |
			(((value >> 08) & 0xFF) << 16) |
			(((value >> 00) & 0xFF) << 24);

		public static long EndianReverse(this long value) =>
			(((value >> 56) & 0xFF) << 00) |
			(((value >> 48) & 0xFF) << 08) |
			(((value >> 40) & 0xFF) << 16) |
			(((value >> 32) & 0xFF) << 24) |
			(((value >> 24) & 0xFF) << 32) |
			(((value >> 16) & 0xFF) << 40) |
			(((value >> 08) & 0xFF) << 48) |
			(((value >> 00) & 0xFF) << 56);

		public static ushort EndianReverse(this ushort value) {
			unchecked {
				return (ushort)(
					(((value >> 00) & 0xFF) << 08) |
					(((value >> 08) & 0xFF) << 00)
				);
			}
		}

		public static uint EndianReverse(this uint value) =>
			(((value >> 24) & 0xFF) << 00) |
			(((value >> 16) & 0xFF) << 08) |
			(((value >> 08) & 0xFF) << 16) |
			(((value >> 00) & 0xFF) << 24);

		public static ulong EndianReverse(this ulong value) =>
			(((value >> 56) & 0xFF) << 00) |
			(((value >> 48) & 0xFF) << 08) |
			(((value >> 40) & 0xFF) << 16) |
			(((value >> 32) & 0xFF) << 24) |
			(((value >> 24) & 0xFF) << 32) |
			(((value >> 16) & 0xFF) << 40) |
			(((value >> 08) & 0xFF) << 48) |
			(((value >> 00) & 0xFF) << 56);

		public static T EndianReverse<T>(this T value) where T : unmanaged => value switch {
			int v => (T)(object)v.EndianReverse(),
			uint v => (T)(object)v.EndianReverse(),
			long v => (T)(object)v.EndianReverse(),
			ulong v => (T)(object)v.EndianReverse(),
			short v => (T)(object)v.EndianReverse(),
			ushort v => (T)(object)v.EndianReverse(),
			_ => throw new NotImplementedException(),
		};

		public static T ToEndian<T>(this T value, ByteOrder endianness) where T : unmanaged => value switch {
			int v => (T)(object)v.ToEndian(endianness),
			uint v => (T)(object)v.ToEndian(endianness),
			long v => (T)(object)v.ToEndian(endianness),
			ulong v => (T)(object)v.ToEndian(endianness),
			short v => (T)(object)v.ToEndian(endianness),
			ushort v => (T)(object)v.ToEndian(endianness),
			_ => throw new NotImplementedException(),
		};

		public static short ToEndian(this short value, ByteOrder endianness) => endianness switch {
			ByteOrder.BigEndian => IsBigEndian ? value : value.EndianReverse(),
			ByteOrder.LittleEndian => IsLittleEndian ? value : value.EndianReverse(),
			ByteOrder.Native => value,
			_ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
		};

		public static int ToEndian(this int value, ByteOrder endianness) => endianness switch {
			ByteOrder.BigEndian => IsBigEndian ? value : value.EndianReverse(),
			ByteOrder.LittleEndian => IsLittleEndian ? value : value.EndianReverse(),
			ByteOrder.Native => value,
			_ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
		};

		public static long ToEndian(this long value, ByteOrder endianness) => endianness switch {
			ByteOrder.BigEndian => IsBigEndian ? value : value.EndianReverse(),
			ByteOrder.LittleEndian => IsLittleEndian ? value : value.EndianReverse(),
			ByteOrder.Native => value,
			_ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
		};

		public static ushort ToEndian(this ushort value, ByteOrder endianness) => endianness switch {
			ByteOrder.BigEndian => IsBigEndian ? value : value.EndianReverse(),
			ByteOrder.LittleEndian => IsLittleEndian ? value : value.EndianReverse(),
			ByteOrder.Native => value,
			_ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
		};

		public static uint ToEndian(this uint value, ByteOrder endianness) => endianness switch {
			ByteOrder.BigEndian => IsBigEndian ? value : value.EndianReverse(),
			ByteOrder.LittleEndian => IsLittleEndian ? value : value.EndianReverse(),
			ByteOrder.Native => value,
			_ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
		};

		public static ulong ToEndian(this ulong value, ByteOrder endianness) => endianness switch {
			ByteOrder.BigEndian => IsBigEndian ? value : value.EndianReverse(),
			ByteOrder.LittleEndian => IsLittleEndian ? value : value.EndianReverse(),
			ByteOrder.Native => value,
			_ => throw new ArgumentOutOfRangeException(nameof(endianness), endianness, null),
		};
	}
}
