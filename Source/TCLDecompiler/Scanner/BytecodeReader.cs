using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static TCLDecompiler.Predicates;

namespace TCLDecompiler {
	public delegate T DecoderCb<T>();
	public struct BytecodeReader {
		private Reader<char> _reader;
		public BytecodeReader(string input) => _reader = new Reader<char>(input.AsMemory());

		public bool IsAtEnd => _reader.IsAtEnd;

		public void ConsumeWhitespace() => _reader.AdvanceWhile(SyntaxFacts.IsWhitespaceOrNewline);
		public bool Match(string input) => _reader.TryMatch(input);
		public char ReadChar() {
			ConsumeWhitespace();
			return _reader.ReadOne();
		}

		public int ReadInteger() {
			if (!TryReadInteger(out int value)) throw new FormatException("Could not read integer from data");
			return value;
		}

		public bool TryReadInteger(out int value) {
			ConsumeWhitespace();
			int savedPosition = _reader.Position;

			if (_reader.IsNext(SyntaxFacts.IsPlusOrMinus))
				_reader.Advance();

			_reader.AdvanceWhile(SyntaxFacts.IsDigit);

			if (!int.TryParse(_reader.ConsumedSpan[savedPosition..], out value)) {
				_reader.Position = savedPosition;
				return false;
			}

			return true;
		}


		public double ReadDouble() {
			if (!TryReadDouble(out double value)) throw new FormatException("Could not read integer from data");
			return value;
		}

		public bool TryReadDouble(out double value) {
			ConsumeWhitespace();
			int savedPosition = _reader.Position;

			if (_reader.IsNext(SyntaxFacts.IsPlusOrMinus))
				_reader.Advance();

			_reader.AdvanceWhile(SyntaxFacts.IsDigit);

			if (_reader.TryRead(Chars.Dot))
				_reader.AdvanceWhile(SyntaxFacts.IsDigit);

			if (_reader.TryRead(SyntaxFacts.IsExponentSign)) {
				_reader.TryRead(SyntaxFacts.IsPlusOrMinus);
				_reader.AdvanceWhile(SyntaxFacts.IsDigit);
			}

			if (!double.TryParse(_reader.ConsumedSpan[savedPosition..], out value)) {
				_reader.Position = savedPosition;
				return false;
			}

			return true;
		}

		public T[] DecodeArray<T>(int? expectedCount, DecoderCb<T> callback) {
			if (callback == null) throw new ArgumentNullException(nameof(callback));

			int count = ReadInteger();
			if (expectedCount.HasValue && expectedCount.Value != count) throw new InvalidDataException("count != expectedCount");

			var results = new T[count];
			for (int i = 0; i < count; i++) results[i] = callback();
			return results;
		}

		public string ReadWord() {
			ConsumeWhitespace();
			return _reader.ReadUntil(SyntaxFacts.IsWhitespaceOrNewline).ToString();
		}

		public string ReadLine() {
			ConsumeWhitespace();
			string result = _reader.ReadUntil(SyntaxFacts.IsNewline).ToString();
			_reader.AdvanceWhile(SyntaxFacts.IsNewline);
			return result;
		}

		public string ReadString() {
			ConsumeWhitespace();
			int length = ReadInteger();
			return _reader.Read(length).ToString();
		}

		public string DecodeString() => Encoding.ASCII.GetString(DecodeData().AsSpan());
		public byte[] DecodeData() => DecodeData(null);
		public byte[] DecodeData(int? expectedLength) {
			ConsumeWhitespace();
			int length = ReadInteger();
			if (expectedLength.HasValue && length != expectedLength.Value) throw new InvalidDataException("not of the expected length");
			return Decode(length);
		}

		public byte[] Decode(int length) {
			int offset = 0;
			int remaining = length;

			byte[] result = new byte[length];

			Span<sbyte> buffer = stackalloc sbyte[5];
			int index = 0;

			static int times85(int x) => (((((x << 2) + x) << 2) + x) << 2) + x;

			while (remaining > 0) {
				int cidx = Convert.ToInt32(ReadChar());
				sbyte code = _decodeMap[cidx];

				if (code == _a85Whitespace) continue;
				if (code == _a85IllegalChar) throw new InvalidDataException("illegal character");

				if (code == _a85Z) {
					if (index != 0) throw new InvalidDataException("malformed bytes");
					result[offset++] = 0; remaining -= 4;
				}
				else {
					buffer[index++] = code;

					if (remaining >= 4 && index > 4) {
						int word = buffer[4];
						for (int i = 3; i >= 0; i--) word = times85(word) + buffer[i];

						result[offset++] = (byte)(word & 0xff); remaining--;
						result[offset++] = (byte)((word >> 8) & 0xff); remaining--;
						result[offset++] = (byte)((word >> 16) & 0xff); remaining--;
						result[offset++] = (byte)((word >> 24) & 0xff); remaining--;

						index = 0;
					}
					else if (index > remaining) {
						for (int i = remaining + 1; i < 5; i++) buffer[i] = 0;

						int word = buffer[4];
						for (int i = 3; i >= 0; i--) word = times85(word) + buffer[i];

						result[offset++] = (byte)(word & 0xff); remaining--;
						if (remaining > 0) { result[offset++] = (byte)((word >> 8) & 0xff); remaining--; }
						if (remaining > 0) { result[offset++] = (byte)((word >> 16) & 0xff); remaining--; }
						if (remaining > 0) { result[offset++] = (byte)((word >> 24) & 0xff); remaining--; }

						index = 0;
					}
				}
			}

			return result;
		}

		private const sbyte _a85Whitespace = -1;
		private const sbyte _a85IllegalChar = -2;
		private const sbyte _a85Z = -3;
		private static readonly sbyte[] _decodeMap = new sbyte[] {
			_a85IllegalChar, /* ^@ */
			_a85IllegalChar, /* ^A */
			_a85IllegalChar, /* ^B */
			_a85IllegalChar, /* ^C */
			_a85IllegalChar, /* ^D */
			_a85IllegalChar, /* ^E */
			_a85IllegalChar, /* ^F */
			_a85IllegalChar, /* ^G */
			_a85IllegalChar, /* ^H */
			_a85Whitespace,   /* \t */
			_a85Whitespace,   /* \n */
			_a85IllegalChar, /* ^K */
			_a85IllegalChar, /* ^L */
			_a85Whitespace,   /* \r */
			_a85IllegalChar, /* ^N */
			_a85IllegalChar, /* ^O */
			_a85IllegalChar, /* ^P */
			_a85IllegalChar, /* ^Q */
			_a85IllegalChar, /* ^R */
			_a85IllegalChar, /* ^S */
			_a85IllegalChar, /* ^T */
			_a85IllegalChar, /* ^U */
			_a85IllegalChar, /* ^V */
			_a85IllegalChar, /* ^W */
			_a85IllegalChar, /* ^X */
			_a85IllegalChar, /* ^Y */
			_a85IllegalChar, /* ^Z */
			_a85IllegalChar, /* ^[ */
			_a85IllegalChar, /* ^\ */
			_a85IllegalChar, /* ^] */
			_a85IllegalChar, /* ^^ */
			_a85IllegalChar, /* ^_ */
			_a85Whitespace,   /*   */
			0,                /* ! */
			_a85IllegalChar, /* " (for hilit: ") */
			2,                /* # */
			_a85IllegalChar, /* $ */
			4,                /* % */
			5,                /* & */
			6,                /* ' */
			7,                /* ( */
			8,                /* ) */
			9,                /* * */
			10,               /* + */
			11,               /* , */
			12,               /* - */
			13,               /* . */
			14,               /* / */
			15,               /* 0 */
			16,               /* 1 */
			17,               /* 2 */
			18,               /* 3 */
			19,               /* 4 */
			20,               /* 5 */
			21,               /* 6 */
			22,               /* 7 */
			23,               /* 8 */
			24,               /* 9 */
			25,               /* : */
			26,               /* ; */
			27,               /* < */
			28,               /* = */
			29,               /* > */
			30,               /* ? */
			31,               /* @ */
			32,               /* A */
			33,               /* B */
			34,               /* C */
			35,               /* D */
			36,               /* E */
			37,               /* F */
			38,               /* G */
			39,               /* H */
			40,               /* I */
			41,               /* J */
			42,               /* K */
			43,               /* L */
			44,               /* M */
			45,               /* N */
			46,               /* O */
			47,               /* P */
			48,               /* Q */
			49,               /* R */
			50,               /* S */
			51,               /* T */
			52,               /* U */
			53,               /* V */
			54,               /* W */
			55,               /* X */
			56,               /* Y */
			57,               /* Z */
			_a85IllegalChar, /* [ */
			_a85IllegalChar, /* \ */
			_a85IllegalChar, /* ] */
			61,               /* ^ */
			62,               /* _ */
			63,               /* ` */
			64,               /* a */
			65,               /* b */
			66,               /* c */
			67,               /* d */
			68,               /* e */
			69,               /* f */
			70,               /* g */
			71,               /* h */
			72,               /* i */
			73,               /* j */
			74,               /* k */
			75,               /* l */
			76,               /* m */
			77,               /* n */
			78,               /* o */
			79,               /* p */
			80,               /* q */
			81,               /* r */
			82,               /* s */
			83,               /* t */
			84,               /* u */
			1,                /* v (replaces ") " */
			3,                /* w (replaces $) */
			58,               /* x (replaces [) */
			59,               /* y (replaces \) */
			_a85Z,            /* z */
			_a85IllegalChar, /* { */
			60,               /* | (replaces ]) */
			_a85IllegalChar, /* } */
			_a85IllegalChar, /* ~ */
		};
	}
}
