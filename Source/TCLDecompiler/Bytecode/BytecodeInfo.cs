using System;

namespace TCLDecompiler {
	public readonly struct BytecodeInfo {
		public BytecodeInfo(int numCommands, int numSrcBytes, int numCodeBytes, int numLitObjects, int numExceptRanges, int numAuxDataItems, int numCmdLocBytes, int maxExceptDepth, int maxStackDepth, int codeDeltaSize, int codeLengthSize, int srcDeltaSize, int srcLengthSize) : this() {
			NumCommands = numCommands;
			NumSrcBytes = numSrcBytes;
			NumCodeBytes = numCodeBytes;
			NumLitObjects = numLitObjects;
			NumExceptRanges = numExceptRanges;
			NumAuxDataItems = numAuxDataItems;
			NumCmdLocBytes = numCmdLocBytes;
			MaxExceptDepth = maxExceptDepth;
			MaxStackDepth = maxStackDepth;
			CodeDeltaSize = codeDeltaSize;
			CodeLengthSize = codeLengthSize;
			SrcDeltaSize = srcDeltaSize;
			SrcLengthSize = srcLengthSize;
		}

		public readonly int NumCommands;
		public readonly int NumSrcBytes;
		public readonly int NumCodeBytes;
		public readonly int NumLitObjects;
		public readonly int NumExceptRanges;
		public readonly int NumAuxDataItems;
		public readonly int NumCmdLocBytes;
		public readonly int MaxExceptDepth;
		public readonly int MaxStackDepth;
		public readonly int CodeDeltaSize;
		public readonly int CodeLengthSize;
		public readonly int SrcDeltaSize;
		public readonly int SrcLengthSize;

		public override bool Equals(object? obj) => obj is BytecodeInfo info &&
				   NumCommands == info.NumCommands &&
				   NumSrcBytes == info.NumSrcBytes &&
				   NumCodeBytes == info.NumCodeBytes &&
				   NumLitObjects == info.NumLitObjects &&
				   NumExceptRanges == info.NumExceptRanges &&
				   NumAuxDataItems == info.NumAuxDataItems &&
				   NumCmdLocBytes == info.NumCmdLocBytes &&
				   MaxExceptDepth == info.MaxExceptDepth &&
				   MaxStackDepth == info.MaxStackDepth &&
				   CodeDeltaSize == info.CodeDeltaSize &&
				   CodeLengthSize == info.CodeLengthSize &&
				   SrcDeltaSize == info.SrcDeltaSize &&
				   SrcLengthSize == info.SrcLengthSize;

		public override int GetHashCode() {
			var hash = new HashCode();
			hash.Add(NumCommands);
			hash.Add(NumSrcBytes);
			hash.Add(NumCodeBytes);
			hash.Add(NumLitObjects);
			hash.Add(NumExceptRanges);
			hash.Add(NumAuxDataItems);
			hash.Add(NumCmdLocBytes);
			hash.Add(MaxExceptDepth);
			hash.Add(MaxStackDepth);
			hash.Add(CodeDeltaSize);
			hash.Add(CodeLengthSize);
			hash.Add(SrcDeltaSize);
			hash.Add(SrcLengthSize);
			return hash.ToHashCode();
		}
	}
}
