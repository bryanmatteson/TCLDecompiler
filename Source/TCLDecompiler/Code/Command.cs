using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TCLDecompiler {
	public interface ICommand: ICodeUnit, ISourceRepresentable, IFormattable {
		void Append(ICodeUnit unit);
	}

	public class Command: CodeBlock, ICommand {
		private IFormattable _formatter;
		public Command(IEnumerable<ICodeUnit> units, IFormattable fmt) : base(units) => _formatter = fmt;
		public Command(ICodeUnit unit, IFormattable fmt) : base(unit) => _formatter = fmt;

		public void Format(ICodeFormatter formatter) => _formatter.Format(formatter);

		public string ToSourceString() {
			var formatter = new CodeFormatter();
			formatter.Write(_formatter);
			string result = formatter.ToString();
			if (StackEffect > 0) {
				result = "[" + result + "]";
			}
			return result;
		}
	}
}
