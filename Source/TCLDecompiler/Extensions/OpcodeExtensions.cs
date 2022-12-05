using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public static class OpcodeExtensions {
		private static readonly Opcode[] _unconditionalJumps = new[] { Opcode.Jump1, Opcode.Jump4, Opcode.Continue, Opcode.Break };
		private static readonly Opcode[] _conditionalJumps = new[] { Opcode.JumpTrue1, Opcode.JumpTrue4, Opcode.JumpFalse1, Opcode.JumpFalse4 };
		private static readonly Opcode[] _allBranches = _unconditionalJumps.Concat(_conditionalJumps).ToArray();

		public static bool IsConditionalBranch(this Opcode opcode) => _conditionalJumps.Contains(opcode);
		public static bool IsUnconditionalBranch(this Opcode opcode) => _unconditionalJumps.Contains(opcode);
		public static bool IsBranch(this Opcode opcode) => _allBranches.Contains(opcode);
		public static BranchType GetBranchType(this Opcode opcode)
			=> IsBranch(opcode) ? (IsConditionalBranch(opcode) ? BranchType.Conditional : (IsUnconditionalBranch(opcode) ? BranchType.Unconditional : BranchType.None)) : BranchType.None;
	}
}
