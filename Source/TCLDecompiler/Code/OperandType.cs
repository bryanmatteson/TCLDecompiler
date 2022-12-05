using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public enum OperandType: int {
		None = 0,
		Int1, Int4,
		UInt1, UInt4,
		Idx4,
		Lvt1, Lvt4,
		Aux4,
		Offset1, Offset4,
		Lit1, Lit4,
		Scls1
	}

	public static class OpTypeExtensions {
		public static int MemSize(this OperandType opType) => opType switch {
			OperandType.Int1 or OperandType.UInt1 or OperandType.Lvt1 or OperandType.Offset1 or OperandType.Lit1 or OperandType.Scls1 => 1,
			OperandType.Int4 or OperandType.UInt4 or OperandType.Lvt4 or OperandType.Idx4 or OperandType.Aux4 or OperandType.Offset4 or OperandType.Lit4 => 4,
			_ => 0,
		};
	}
}
