using System;
using System.Text;

namespace TCLDecompiler {
	public interface ICodeFormatter {
		ICodeFormatter WriteDelimited(string delimiter, IFormattable[] formats);
		ICodeFormatter Write(IFormattable format);
		ICodeFormatter Write(string msg, params object[] args);
		ICodeFormatter WriteLine(string msg, params object[] args);
		ICodeFormatter WriteLineIndent(string msg, params object[] args);
		ICodeFormatter NewLine();
		ICodeFormatter NewLineIfNeeded();
		ICodeFormatter NeedNewLine();
		ICodeFormatter ResetNewLine();
		ICodeFormatter Indent(uint indentation = 1);
		ICodeFormatter Unindent();
		ICodeFormatter WriteOpenBraceAndIndent();
		ICodeFormatter UnindentAndWriteCloseBrace();
	}
}
