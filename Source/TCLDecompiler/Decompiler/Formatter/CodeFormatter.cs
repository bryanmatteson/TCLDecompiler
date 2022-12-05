using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TCLDecompiler {
	public class CodeFormatter: ICodeFormatter {
		public const uint DefaultIndentation = 4;
		public StringBuilder Builder { get; }
		public bool IsStartOfLine { get; private set; }
		public bool NeedsNewLine { get; private set; }
		public uint CurrentIndentation { get; private set; }

		public CodeFormatter() => Builder = new StringBuilder();

		public ICodeFormatter Write(string msg, params object[] args) {
			if (string.IsNullOrEmpty(msg)) return this;

			if (args.Length > 0) msg = string.Format(msg, args);
			if (IsStartOfLine && !string.IsNullOrWhiteSpace(msg)) Builder.Append(new string(' ', (int)(CurrentIndentation * DefaultIndentation)));
			if (msg.Length > 0) IsStartOfLine = msg.EndsWith(Environment.NewLine);

			Builder.Append(msg);
			return this;
		}

		public ICodeFormatter Write(IFormattable format) {
			format.Format(this);
			return this;
		}

		public ICodeFormatter WriteDelimited(string delimiter, params IFormattable[] formats) {
			if (formats.Length == 0) return this;
			if (formats.Length == 1) return Write(formats[0]);

			for (int i = 0; i < formats.Length - 1; i++) {
				Write(formats[i]).Write(delimiter);
			}
			return Write(formats[^1]);
		}

		public ICodeFormatter WriteLine(string msg, params object[] args) {
			Write(msg, args);
			NewLine();
			return this;
		}

		public ICodeFormatter WriteLineIndent(string msg, params object[] args) {
			Indent();
			WriteLine(msg, args);
			Unindent();
			return this;
		}

		public ICodeFormatter NewLine() {
			Builder.AppendLine(string.Empty);
			IsStartOfLine = true;
			return this;
		}

		public ICodeFormatter NewLineIfNeeded() {
			if (!NeedsNewLine) return this;
			NewLine();
			NeedsNewLine = false;
			return this;
		}

		public ICodeFormatter NeedNewLine() {
			NeedsNewLine = true;
			return this;
		}

		public ICodeFormatter ResetNewLine() {
			NeedsNewLine = false;
			return this;
		}

		public ICodeFormatter Indent(uint indentation = 1) {
			CurrentIndentation += indentation;
			return this;
		}

		public ICodeFormatter Unindent() {
			CurrentIndentation--;
			return this;
		}

		public ICodeFormatter WriteOpenBraceAndIndent() {
			WriteLine(" {");
			Indent();
			return this;
		}

		public ICodeFormatter UnindentAndWriteCloseBrace() {
			Unindent();
			WriteLine("}");
			return this;
		}

		public override string ToString() => Builder.ToString();
	}
}
