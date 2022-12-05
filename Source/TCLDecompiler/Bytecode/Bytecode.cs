using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TCLDecompiler {
	public readonly struct Bytecode {
		public readonly BytecodeType Type;
		public readonly BytecodeInfo Info;
		public readonly byte[] Code;
		public readonly CodeRange[] CodeRanges;
		public readonly CodeRange[] SourceRanges;
		public readonly Literal[] Literals;
		public readonly ExceptionRange[] ExceptionRanges;
		public readonly AuxData[] AuxiliaryData;
		public readonly int NumArgs;
		public readonly Local[] Locals;

		public Bytecode(BytecodeType type, BytecodeInfo info, byte[] code, CodeRange[] codeRanges, CodeRange[] sourceRanges, Literal[] literals, ExceptionRange[] exceptionRanges, AuxData[] auxiliaryData, int numArgs, Local[] locals) {
			Type = type;
			Info = info;
			Code = code;
			CodeRanges = codeRanges;
			SourceRanges = sourceRanges;
			Literals = literals;
			ExceptionRanges = exceptionRanges;
			AuxiliaryData = auxiliaryData;
			NumArgs = numArgs;
			Locals = locals;
		}
	}
}
