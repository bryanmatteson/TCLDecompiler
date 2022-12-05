using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TCLDecompiler {
	public class BytecodeParser {
		public const string TclByteCodeMagic = "TclPro ByteCode";
		private BytecodeReader _reader;
		public Header Header { get; }
		public string Code { get; }

		public BytecodeParser(string compiledCode) {
			Code = compiledCode;
			_reader = new BytecodeReader(compiledCode);
			Header = ParseHeader();
		}

		public Bytecode Parse() => ParseBytecode();

		private Bytecode ParseProcBody() => ParseBytecode(BytecodeType.Proc);
		private Bytecode ParseBytecode() => ParseBytecode(BytecodeType.Code);

		private AuxData ParseAuxData() {
			var type = (AuxDataType)_reader.ReadChar();
			object value = type switch {
				AuxDataType.Foreach => ParseForeachInfo(),
				AuxDataType.DictUpdate => ParseDictUpdateInfo(),
				AuxDataType.JumpTable => ParseJumptableInfo(),
				AuxDataType.NewForeach => ParseForeachInfo(),
				_ => throw new InvalidDataException("unknown aux data type")
			};
			return new AuxData(type, value);
		}

		private Bytecode ParseBytecode(BytecodeType type) {
			BytecodeInfo info = ParseBytecodeInfo();
			byte[] code = _reader.DecodeData(info.NumCodeBytes);
			CodeRange[] codeRanges = ParseCodeRanges(info.NumCommands, info.CodeDeltaSize, info.CodeLengthSize);
			CodeRange[] sourceRanges = ParseCodeRanges(info.NumCommands, info.SrcDeltaSize, info.SrcLengthSize);

			Literal[] literals = _reader.DecodeArray(info.NumLitObjects, ParseLiteral);
			ExceptionRange[] exceptionRanges = _reader.DecodeArray(info.NumExceptRanges, ParseExceptionRange);
			AuxData[] auxiliaryData = _reader.DecodeArray(info.NumAuxDataItems, ParseAuxData);
			int numArgs = 0;
			Local[] locals = Array.Empty<Local>();

			if (type == BytecodeType.Proc) {
				numArgs = _reader.ReadInteger();
				int numLocals = _reader.ReadInteger();

				locals = new Local[numLocals];
				for (int i = 0; i < numLocals; i++) {
					locals[i] = ParseLocal();
				}
			}

			return new Bytecode(
				type: type,
				info: info,
				code: code,
				codeRanges: codeRanges,
				sourceRanges: sourceRanges,
				literals: literals,
				exceptionRanges: exceptionRanges,
				auxiliaryData: auxiliaryData,
				numArgs: numArgs,
				locals: locals
			);
		}

		private CodeRange[] ParseCodeRanges(int count, int deltaSize, int lengthSize) {
			byte[] deltas = deltaSize > 0 ? _reader.DecodeData(deltaSize) : Array.Empty<byte>();
			byte[] lengths = lengthSize > 0 ? _reader.DecodeData(lengthSize) : Array.Empty<byte>();

			if (deltas.Length == 0 || lengths.Length == 0 || count <= 0) return Array.Empty<CodeRange>();

			var deltaReader = new BinReader(deltas, ByteOrder.BigEndian);
			var lengthReader = new BinReader(lengths, ByteOrder.BigEndian);

			int offset = 0;
			var result = new CodeRange[count];
			for (int i = 0; i < count; i++) {
				int delta = deltaReader.ReadByte();
				if (delta == 0xff) delta = deltaReader.ReadNumber<int>();
				offset += delta;

				int length = lengthReader.ReadByte();
				if (length == 0xff) length = lengthReader.ReadNumber<int>();
				result[i] = new CodeRange(offset, length);
			}

			return result;
		}

		private BytecodeInfo ParseBytecodeInfo() {
			int numCommands = _reader.ReadInteger();
			int numSrcBytes = _reader.ReadInteger();
			int numCodeBytes = _reader.ReadInteger();
			int numLitObjects = _reader.ReadInteger();
			int numExceptRanges = _reader.ReadInteger();
			int numAuxDataItems = _reader.ReadInteger();
			int numCmdLocBytes = _reader.ReadInteger();
			int maxExceptDepth = _reader.ReadInteger();
			int maxStackDepth = _reader.ReadInteger();
			int codeDeltaSize = _reader.ReadInteger();
			int codeLengthSize = _reader.ReadInteger();
			int srcDeltaSize = _reader.ReadInteger();
			int srcLengthSize = _reader.ReadInteger();

			return new BytecodeInfo(
				numCommands: numCommands,
				numSrcBytes: numSrcBytes,
				numCodeBytes: numCodeBytes,
				numLitObjects: numLitObjects,
				numExceptRanges: numExceptRanges,
				numAuxDataItems: numAuxDataItems,
				numCmdLocBytes: numCmdLocBytes,
				maxExceptDepth: maxExceptDepth,
				maxStackDepth: maxStackDepth,
				codeDeltaSize: codeDeltaSize,
				codeLengthSize: codeLengthSize,
				srcDeltaSize: srcDeltaSize,
				srcLengthSize: srcLengthSize
			);
		}

		private DictUpdateInfo ParseDictUpdateInfo() {
			int count = _reader.ReadInteger();
			int[] indices = new int[count];

			for (int i = 0; i < count; i++) {
				indices[i] = _reader.ReadInteger();
			}
			return new DictUpdateInfo(indices);
		}

		private ExceptionRange ParseExceptionRange() {
			var type = (ExceptionType)_reader.ReadChar();
			int nestingLevel = _reader.ReadInteger();
			int codeOffset = _reader.ReadInteger();
			int numCodeBytes = _reader.ReadInteger();
			int breakOffset = _reader.ReadInteger();
			int continueOffset = _reader.ReadInteger();
			int catchOffset = _reader.ReadInteger();

			return new ExceptionRange(
				type: type,
				nestingLevel: nestingLevel,
				codeOffset: codeOffset,
				numCodeBytes: numCodeBytes,
				breakOffset: breakOffset,
				continueOffset: continueOffset,
				catchOffset: catchOffset
			);
		}

		private ForeachInfo ParseForeachInfo() {
			int numLists = _reader.ReadInteger();
			int firstValue = _reader.ReadInteger();
			int loopCounter = _reader.ReadInteger();

			int[][] lists = new int[numLists][];
			for (int i = 0; i < numLists; i++) {
				int numVars = _reader.ReadInteger();
				int[] varList = new int[numVars];
				for (int j = 0; j < numVars; j++) {
					varList[j] = _reader.ReadInteger();
				}
				lists[i] = varList;
			}

			return new ForeachInfo(firstValue, loopCounter, lists);
		}

		private Header ParseHeader() {
			if (!_reader.Match(TclByteCodeMagic)) throw new InvalidDataException("unexpected header value");

			int format = _reader.ReadInteger();
			int build = _reader.ReadInteger();
			Version compilerVersion = ParseVersion();
			Version tclVersion = ParseVersion();

			return new Header(
				format: format,
				build: build,
				compilerVersion: compilerVersion,
				tclVersion: tclVersion
			);
		}

		private JumptableInfo ParseJumptableInfo() {
			var dict = new Dictionary<string, int>();
			int numJumps = _reader.ReadInteger();
			for (int i = 0; i < numJumps; i++) {
				int value = _reader.ReadInteger();
				string key = _reader.DecodeString();
				dict[key] = value;
			}
			return new JumptableInfo(dict);
		}

		private Literal ParseLiteral() {
			var type = (LiteralType)_reader.ReadChar();
			object value = type switch {
				LiteralType.Boolean => _reader.ReadWord(),
				LiteralType.Bytecode => ParseBytecode(),
				LiteralType.Double => _reader.ReadDouble(),
				LiteralType.Int => _reader.ReadInteger(),
				LiteralType.ProcBody => ParseProcBody(),
				LiteralType.String => _reader.ReadString(),
				LiteralType.XString => _reader.DecodeString(),
				_ => throw new InvalidOperationException("unknown literal type")
			};

			return new Literal(type, value);
		}

		private Local ParseLocal() {
			string name = _reader.DecodeString();
			int frameIndex = _reader.ReadInteger();
			bool hasDefaultValue = _reader.ReadInteger() != 0;
			int flagMask = _reader.ReadInteger();
			Literal defaultValue = hasDefaultValue ? ParseLiteral() : default;
			return new Local(name: name, hasDefaultValue: hasDefaultValue, defaultValue: defaultValue, frameIndex: frameIndex, flagMask: flagMask);
		}

		private Version ParseVersion() {
			int major = _reader.ReadInteger();
			if (!_reader.Match(".")) throw new InvalidDataException("unexpected input");
			int minor = _reader.ReadInteger();
			return new Version(major, minor);
		}
	}
}
