using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TCLDecompiler {
	public partial class TBCFile {
		[GeneratedRegex(@"tbcload::bceval.*\{([^}]*)", RegexOptions.Compiled | RegexOptions.Multiline)]
		public static partial Regex TbcLoadPattern();

		public Bytecode Bytecode { get; }
		public Header Header { get; }

		public TBCFile(string fileName) {
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentException($"'{nameof(fileName)}' cannot be null or empty", nameof(fileName));
			if (!File.Exists(fileName)) throw new ArgumentException($"'{fileName}' does not exist");

			string text = File.ReadAllText(fileName);
			Match? match = TbcLoadPattern().Matches(text).FirstOrDefault(m => m.Success);
			if (match?.Groups.Values.All(g => g.Success) != true) throw new InvalidDataException("not bytecode");

			var parser = new BytecodeParser(match.Groups[1].Value.Trim());
			Bytecode = parser.Parse();
		}
	}
}
