using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct ExceptionRange: IEquatable<ExceptionRange> {
		public static readonly ExceptionRange Default;

		public readonly ExceptionType Type;
		public readonly int NestingLevel;
		public readonly int CodeOffset;
		public readonly int NumCodeBytes;
		public readonly int BreakOffset;
		public readonly int ContinueOffset;
		public readonly int CatchOffset;
		public readonly CodeRange CodeRange;

		public ExceptionRange(ExceptionType type, int nestingLevel, int codeOffset, int numCodeBytes, int breakOffset, int continueOffset, int catchOffset) {
			Type = type;
			NestingLevel = nestingLevel;
			CodeOffset = codeOffset;
			NumCodeBytes = numCodeBytes;
			BreakOffset = breakOffset;
			ContinueOffset = continueOffset;
			CatchOffset = catchOffset;
			CodeRange = new CodeRange(CodeOffset, NumCodeBytes);
		}

		public override bool Equals(object? obj) => obj is ExceptionRange range && Equals(range);

		public bool Equals(ExceptionRange range) => Type == range.Type &&
				   NestingLevel == range.NestingLevel &&
				   CodeOffset == range.CodeOffset &&
				   NumCodeBytes == range.NumCodeBytes &&
				   BreakOffset == range.BreakOffset &&
				   ContinueOffset == range.ContinueOffset &&
				   CatchOffset == range.CatchOffset &&
				   CodeRange.Equals(range.CodeRange);

		public override int GetHashCode() => HashCode.Combine(Type, NestingLevel, CodeOffset, NumCodeBytes, BreakOffset, ContinueOffset, CatchOffset, CodeRange);

		public static bool operator ==(ExceptionRange lhs, ExceptionRange rhs) => lhs.Equals(rhs);
		public static bool operator !=(ExceptionRange lhs, ExceptionRange rhs) => !lhs.Equals(rhs);
	}
}
