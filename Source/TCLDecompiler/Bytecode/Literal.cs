using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct Literal: IEquatable<Literal>, ISourceRepresentable {
		public readonly LiteralType Type;
		public readonly object Value;

		public Literal(LiteralType type, object value) {
			Type = type;
			Value = value;
		}

		public override bool Equals(object? obj) => obj is Literal literal && Equals(literal);
		public bool Equals([AllowNull] Literal literal) => Type == literal.Type && Value.Equals(literal.Value);
		public override int GetHashCode() => HashCode.Combine(Type, Value);

		public string ToSourceString() {
			switch (Type) {
				case LiteralType.String:
				case LiteralType.XString:
					if (Value is string str) return string.IsNullOrEmpty(str) ? "\"\"" : str;
					return Value.ToString() ?? string.Empty;

				case LiteralType.Bytecode:
					return "Bytecode";

				case LiteralType.ProcBody:
					return "Procbody";

				case LiteralType.Boolean:
				case LiteralType.Double:
				case LiteralType.Int:
				default:
					return Value.ToString() ?? string.Empty;
			}
		}

		public override string ToString() => ToSourceString();
	}
}
