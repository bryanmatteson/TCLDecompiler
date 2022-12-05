using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct Instruction: ICodeUnit, IEquatable<Instruction> {
		public readonly InstrDesc Descriptor => TclTable.InstructionDescriptors[Opcode];
		public readonly string Name => Descriptor.Name;
		public readonly int Size => Descriptor.ByteCount;
		public readonly CodeRange Range => new(Location, Size);
		public readonly int StackEffect => Descriptor.StackEffect == int.MinValue ? (1 - (int)Operands[0]) : Descriptor.StackEffect;
		public readonly int ArgumentCount => Descriptor.ArgumentCount == int.MinValue ? (int)Operands[0] : Descriptor.ArgumentCount;
		public readonly int Location { get; }
		public readonly IEnumerable<BranchTarget> Targets {
			get {
				if (Opcode.IsBranch()) yield return new BranchTarget(Location + (int)Operands[0], Opcode.IsUnconditionalBranch() ? BranchType.Unconditional : BranchType.Conditional);
				if (!Opcode.IsUnconditionalBranch()) yield return new BranchTarget(Location + Size, BranchType.Fallthrough);
			}
		}

		public readonly object[] Operands;
		public readonly Opcode Opcode;

		public Instruction(int location, Opcode opcode, object[] operands) {
			Operands = operands;
			Opcode = opcode;
			Location = location;
		}

		public override string ToString() {
			var builder = new StringBuilder();
			builder.Append(Location).Append(' ').Append(Name).Append(' ').AppendJoin(' ', Operands.Select(o => o.ToString()));
			if (Opcode.IsBranch()) builder.Append(" # pc ").Append(Location + (int)Operands[0]);
			return builder.ToString();
		}

		public override bool Equals(object? obj) => obj is Instruction other && Equals(other);
		public bool Equals(Instruction other) => Location == other.Location && Opcode == other.Opcode && Operands?.SequenceEqual(other.Operands) == true;
		public override int GetHashCode() => HashCode.Combine(Location, Opcode, Operands);
		public static bool operator ==(Instruction left, Instruction right) => left.Equals(right);
		public static bool operator !=(Instruction left, Instruction right) => !(left == right);
		public static bool operator >(Instruction left, Instruction right) => left.Location > right.Location;
		public static bool operator <(Instruction left, Instruction right) => left.Location < right.Location;
		public static bool operator >=(Instruction left, Instruction right) => left.Location >= right.Location;
		public static bool operator <=(Instruction left, Instruction right) => left.Location <= right.Location;
	}
}
