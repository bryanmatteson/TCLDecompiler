using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public class Decompiler {
		public DecompilerOptions Options { get; }
		public Decompiler(DecompilerOptions options) => Options = options;

		public string Decompile(Bytecode bytecode) {
			if (Options.Verbose) Diagnostics.Level = DiagnosticKind.Debug;

			IEnumerable<Instruction> instructions = bytecode.Disassemble();
			var codeMap = new CodeMap(instructions);

			InstructionReducer.Reduce(codeMap);
			CatchReducer.Reduce(codeMap);
			LogicalsReducer.Reduce(codeMap);


			foreach (ICodeUnit unit in codeMap.Units) {
				if (unit is ICommand command) {
					Console.WriteLine(command.ToSourceString());
				}
				else {
					Console.WriteLine(unit);
				}
			}

			return "";
		}
	}
}
