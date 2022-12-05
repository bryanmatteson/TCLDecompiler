using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public static class BytecodeDisplayExtensions {
		private static int DigitCount(this int n) => n == 0 ? 1 : (n > 0 ? 1 : 2) + (int)Math.Log10(Math.Abs((double)n));

		public static void Dump(this Bytecode bytecode) {
			Console.WriteLine(bytecode.HeaderString());
			Console.WriteLine(bytecode.ExceptionInfoString());
			Console.WriteLine(bytecode.CommandInfoString());
			Console.WriteLine(bytecode.InstructionsString());
		}

		public static void PrintHeader(this Bytecode bytecode) {
			Console.WriteLine(bytecode.HeaderString());
			Console.WriteLine(bytecode.ExceptionInfoString());
			Console.WriteLine(bytecode.CommandInfoString());
		}

		public static string ExceptionInfoString(this Bytecode bytecode) {
			var builder = new StringBuilder();
			int exceptionTypeWidth = Enum.GetNames(typeof(ExceptionType)).Max(s => s.Length);

			if (bytecode.Info.NumExceptRanges > 0) {
				int continueOffsetWidth = bytecode.ExceptionRanges.Max(r => r.ContinueOffset).DigitCount();
				int codeOffsetLowerWidth = bytecode.ExceptionRanges.Max(r => r.CodeOffset).DigitCount();
				int codeOffsetUpperWidth = bytecode.ExceptionRanges.Max(r => r.CodeOffset + r.NumCodeBytes).DigitCount();
				int breakOffsetWidth = bytecode.ExceptionRanges.Max(r => r.BreakOffset).DigitCount();
				int catchOffsetWidth = bytecode.ExceptionRanges.Max(r => r.CatchOffset).DigitCount();

				builder.Append("Exception ranges ").Append(bytecode.Info.NumExceptRanges).Append(", depth ").Append(bytecode.Info.MaxExceptDepth).Append('\n');

				foreach (ExceptionRange exception in bytecode.ExceptionRanges) {
					builder.Append("    ").Append(Array.IndexOf(bytecode.ExceptionRanges, exception).ToString().RightJustified(bytecode.ExceptionRanges.Length.DigitCount()))
						.Append(": level ").Append(exception.NestingLevel.ToString().RightJustified(bytecode.Info.MaxExceptDepth.DigitCount()))
						.Append(", ").Append(exception.Type.ToString().RightJustified(exceptionTypeWidth))
						.Append(", pc ").Append(exception.CodeOffset.ToString().RightJustified(codeOffsetLowerWidth)).Append('-').Append((exception.CodeOffset + exception.NumCodeBytes).ToString().LeftJustified(codeOffsetUpperWidth))
						.Append(", continue ").Append(exception.ContinueOffset.ToString().RightJustified(continueOffsetWidth))
						.Append(", break ").Append(exception.BreakOffset.ToString().RightJustified(breakOffsetWidth))
						.Append(", catch ").Append(exception.CatchOffset.ToString().RightJustified(catchOffsetWidth))
						.Append('\n');
				}
			}
			return builder.ToString();
		}

		public static string CommandInfoString(this Bytecode bytecode) {
			var builder = new StringBuilder();
			builder.Append("Commands ").Append(bytecode.Info.NumCommands).Append(":\n");
			int cmdWidth = bytecode.Info.NumCommands.ToString().Length;
			int instWidth = bytecode.Info.NumCodeBytes.ToString().Length;

			foreach (CodeRange codeRange in bytecode.CodeRanges) {
				int idx = Array.IndexOf(bytecode.CodeRanges, codeRange);
				string index = (idx + 1).ToString().RightJustified(cmdWidth);
				string lower = codeRange.Start.ToString().RightJustified(instWidth);
				string upper = codeRange.End.ToString().RightJustified(instWidth);
				builder.Append("    ").Append(index).Append(": pc ").Append(lower).Append('-').Append(upper);
				if ((idx + 1 != bytecode.CodeRanges.Length) && ((idx + 1) % 4 == 0)) builder.Append('\n');
			}

			return builder.ToString();
		}

		public static string HeaderString(this Bytecode bytecode) {
			var builder = new StringBuilder();
			builder.Append("Cmds ").Append(bytecode.Info.NumCommands)
				.Append(", src ").Append(bytecode.Info.NumSrcBytes)
				.Append(", code ").Append(bytecode.Info.NumCodeBytes)
				.Append(", lits ").Append(bytecode.Info.NumLitObjects)
				.Append(", aux ").Append(bytecode.Info.NumAuxDataItems)
				.Append(", stkDepth ").Append(bytecode.Info.MaxStackDepth);
			return builder.ToString();
		}

		public static string InstructionsString(this Bytecode bytecode) {
			var instructions = bytecode.Disassemble().ToList();
			int locationWidth = instructions.Max(i => i.Location).DigitCount();
			int nameWidth = instructions.Max(i => i.Name.Length);
			int opWidth = instructions.Max(i => string.Join(' ', i.Operands.ToArray().Select(o => o.ToString())).Length);
			int descWidth = instructions.Where(i => i.Opcode.IsBranch()).Max(i => i.Location + (int)i.Operands[0]).DigitCount();

			var builder = new StringBuilder();
			foreach (Instruction instruction in instructions) {
				builder.Append(instruction.Location.ToString().RightJustified(locationWidth)).Append(' ');
				builder.Append($"{instruction.Name} {string.Join(' ', instruction.Operands.ToArray().Select(o => o.ToString()))}".LeftJustified(nameWidth + opWidth + 1));
				if (instruction.Opcode.IsBranch()) builder.Append("# pc ").Append((instruction.Location + (int)instruction.Operands[0]).ToString().LeftJustified(descWidth));
				builder.Append('\n');
			}
			return builder.ToString();
		}
	}
}
