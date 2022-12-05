
namespace TCLDecompiler {
	public interface IFormattable {
		void Format(ICodeFormatter formatter);
	}

	public readonly record struct ConcatExpression(string Delimiter, IFormattable[] Args): IFormattable {
		public readonly void Format(ICodeFormatter fmt) {
			for (int i = 0; i < Args.Length; i++) {
				if (i > 0) fmt.Write(Delimiter);
				fmt.Write(Args[i]);
			}
		}
	}

	public readonly record struct ArrayRefExpression(IFormattable VarName, IFormattable Key): IFormattable {
		public readonly void Format(ICodeFormatter fmt) => fmt.Write(VarName).Write("(").Write(Key).Write(")");
	}

	public readonly record struct BodyExpression(IFormattable[] Body): IFormattable {
		public readonly void Format(ICodeFormatter fmt) {
			if (Body.Length == 1) fmt.Write(" {").Write(Body[0]).Write(" }");
			else {
				fmt.WriteOpenBraceAndIndent();
				foreach (var unit in Body) fmt.Write(unit).NewLine();
				fmt.UnindentAndWriteCloseBrace();
			}
		}
	}

	public readonly record struct OperationExpression(Opcode Opcode, params IFormattable[] Args): IFormattable {
		public readonly void Format(ICodeFormatter fmt) {
			string opExpression = _opExpressions[Opcode];
			bool isUnary = Args.Length == 1;
			if (isUnary) fmt.Write(opExpression).Write(Args[0]);
			else fmt.Write(Args[0]).Write(" ").Write(opExpression).Write(" ").Write(Args[1]);
		}

		private static readonly Dictionary<Opcode, string> _opExpressions = new Dictionary<Opcode, string> {
			[Opcode.Gt] = ">",
			[Opcode.Lt] = "<",
			[Opcode.Ge] = ">=",
			[Opcode.Le] = "<=",
			[Opcode.Eq] = "==",
			[Opcode.StrEq] = "==",
			[Opcode.Neq] = "!=",
			[Opcode.Add] = "+",
			[Opcode.Not] = "!",
			[Opcode.Land] = "&&",
			[Opcode.Lor] = "||",
			[Opcode.Bitxor] = "^",
			[Opcode.Bitor] = "|",
			[Opcode.Bitnot] = "~",
			[Opcode.Bitand] = "&",
			[Opcode.Lshift] = "<<",
			[Opcode.Rshift] = ">>",
			[Opcode.Uminus] = "-",
			[Opcode.Uplus] = "+",
			[Opcode.Sub] = "-",
			[Opcode.Mult] = "*",
		};
	}

	public readonly record struct QuotedExpression(IFormattable Value): IFormattable {
		public readonly void Format(ICodeFormatter formatter) => formatter.Write("\"").Write(Value).Write("\"");
	}

	public readonly record struct VariableExpression(IFormattable Variable): IFormattable {
		public readonly void Format(ICodeFormatter fmt) => fmt.Write("$").Write(Variable);
	}

	public readonly record struct Representation(object Value): IFormattable {
		public readonly void Format(ICodeFormatter fmt) {
			if (Value is IFormattable formatter) formatter.Format(fmt);
			else if (Value is ISourceRepresentable repr) fmt.Write(repr.ToSourceString());
			else fmt.Write(Value.ToString() ?? string.Empty);
		}
	}

	public static class CommandFormatters {
		public static IFormattable Rep(object obj) {
			if (obj is IFormattable formatter) return formatter;
			return new Representation(obj);
		}


		public static IFormattable[] Reps(params object[] args) => args.Select(Rep).ToArray();
		public static IFormattable Cat(string delim, params object[] args) => new ConcatExpression(delim, Reps(args));
		public static IFormattable Spaced(params object[] args) => Cat(" ", args);
		public static IFormattable Lit(object obj) => Rep(obj);
		public static IFormattable Var(object rep) => new VariableExpression(Rep(rep));
		public static IFormattable Cmd(params object[] reps) => Spaced(reps);
		public static IFormattable Cmd(string name, params object[] reps) => Cmd(reps.Prepend(name).ToArray());
		public static IFormattable Arr(object varName, object key) => new ArrayRefExpression(Rep(varName), Rep(key));
		public static IFormattable Op(Opcode opcode, params object[] args) => new OperationExpression(opcode, Reps(args));
		public static IFormattable QLit(object arg) => new QuotedExpression(Rep(arg));
		public static IFormattable Body(params object[] args) => new BodyExpression(Reps(args));
		public static IFormattable Catch(params object[] args) => Cmd("catch", Body(args));
	}
}
