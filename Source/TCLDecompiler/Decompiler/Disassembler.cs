using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public static class Disassembler {
		public static IEnumerable<Instruction> Disassemble(this Bytecode bc) {

			var reader = new BinReader(bc.Code.AsMemory(), ByteOrder.BigEndian);
			IEnumerable<ExceptionRange> loops = bc.ExceptionRanges.Where(x => x.Type == ExceptionType.Loop);

			var items = new List<Instruction>();
			while (!reader.IsAtEnd) {
				int location = reader.Location;
				var code = (Opcode)reader.ReadByte();
				if (!Enum.IsDefined(typeof(Opcode), code)) throw new InvalidDataException("invalid opcode");

				InstrDesc descriptor = TclTable.InstructionDescriptors[code];
				object[] operands = new object[4];
				int opCount = 0;

				switch (code) {
					case Opcode.Continue:
						operands[opCount++] = loops.LastOrDefault(l => l.CodeRange.Contains(location)).ContinueOffset - location;
						break;

					case Opcode.Break:
						operands[opCount++] = loops.LastOrDefault(l => l.CodeRange.Contains(location)).BreakOffset - location;
						break;

					default:
						foreach (OperandType opType in descriptor.OperandTypes) {
							int op = ScanOp(ref reader, opType);
							operands[opCount++] = GetOpValue(bc, op, opType);
						}
						break;
				}

				items.Add(new Instruction(location, code, operands.AsSpan(0, opCount).ToArray()));
			}

			return items;
		}

		private static int ScanOp(ref BinReader reader, OperandType type) => type switch {
			OperandType.UInt1 or OperandType.Lit1 or OperandType.Lvt1 or OperandType.Scls1 => reader.ReadByte(),
			OperandType.Int1 or OperandType.Offset1 => (sbyte)reader.ReadByte(),
			OperandType.UInt4 or OperandType.Lit4 or OperandType.Lvt4 => (int)reader.ReadNumber<uint>(),
			OperandType.Int4 or OperandType.Offset4 or OperandType.Idx4 or OperandType.Aux4 => reader.ReadNumber<int>(),
			_ => throw new InvalidDataException("unknown op type"),
		};

		private static object GetOpValue(Bytecode bc, int op, OperandType type) => type switch {
			OperandType.Lit1 or OperandType.Lit4 => bc.Literals[op],
			OperandType.Lvt1 or OperandType.Lvt4 => bc.Locals[op],
			_ => op,
		};
	}
}
