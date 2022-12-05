using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static TCLDecompiler.Predicates;

namespace TCLDecompiler {
	public ref struct BinReader {
		private Reader<byte> _reader;
		private readonly ByteOrder _byteOrder;

		public BinReader(ReadOnlyMemory<byte> input) {
			_reader = new Reader<byte>(input);
			_byteOrder = ByteOrder.Native;
		}

		public bool IsAtEnd => _reader.IsAtEnd;
		public int Location => _reader.Position;

		public BinReader(ReadOnlyMemory<byte> input, ByteOrder byteOrder) : this(input) => _byteOrder = byteOrder;

		public byte ReadByte() => _reader.ReadOne();
		public unsafe T ReadNumber<T>() where T : unmanaged => ReadNumber<T>(_byteOrder);

		public unsafe T ReadNumber<T>(ByteOrder byteOrder) where T : unmanaged {
			int size = sizeof(T);
			if (_reader.Remaining < size) throw new InvalidDataException("out of bounds");
			ReadOnlySpan<byte> bytes = _reader.Read(size);
			T value = MemoryMarshal.Read<T>(bytes);
			return value.ToEndian(byteOrder);
		}
	}
}
