using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TCLDecompiler {
	public enum DiagnosticKind {
		Debug,
		Message,
		Warning,
		Error
	}

	public struct DiagnosticInfo {
		public DiagnosticKind Kind;
		public string Message;
		public string File;
		public int Line;
		public int Column;
	}

	public interface IDiagnostics {
		DiagnosticKind Level { get; set; }
		void Emit(DiagnosticInfo info);
		void PushIndent(int level = 4);
		void PopIndent();
	}

	public static class Diagnostics {
		public static IDiagnostics Implementation { get; set; } = new ConsoleDiagnostics();

		public static DiagnosticKind Level {
			get => Implementation.Level;
			set => Implementation.Level = value;
		}

		public static void PushIndent(int level = 4) => Implementation.PushIndent(level);

		public static void PopIndent() => Implementation.PopIndent();

		public static void Debug(string msg, params object[] args) {
			var diagInfo = new DiagnosticInfo {
				Kind = DiagnosticKind.Debug,
				Message = args.Length > 0 ? string.Format(msg, args) : msg
			};

			Implementation.Emit(diagInfo);
		}

		public static void Message(string msg, params object[] args) {
			var diagInfo = new DiagnosticInfo {
				Kind = DiagnosticKind.Message,
				Message = args.Length > 0 ? string.Format(msg, args) : msg
			};

			Implementation.Emit(diagInfo);
		}

		public static void Warning(string msg, params object[] args) {
			var diagInfo = new DiagnosticInfo {
				Kind = DiagnosticKind.Warning,
				Message = args.Length > 0 ? string.Format(msg, args) : msg
			};

			Implementation.Emit(diagInfo);
		}

		public static void Error(string msg, params object[] args) {
			var diagInfo = new DiagnosticInfo {
				Kind = DiagnosticKind.Error,
				Message = args.Length > 0 ? string.Format(msg, args) : msg
			};

			Implementation.Emit(diagInfo);
		}
	}

	public class ConsoleDiagnostics: IDiagnostics {
		public Stack<int> Indents;
		public DiagnosticKind Level { get; set; }

		public ConsoleDiagnostics() {
			Indents = new Stack<int>();
			Level = DiagnosticKind.Message;
		}

		public void Emit(DiagnosticInfo info) {
			if (info.Kind < Level) return;

			int currentIndentation = Indents.Sum();
			string message = new string(' ', currentIndentation) + info.Message;

			if (info.Kind == DiagnosticKind.Error) Console.Error.WriteLine(message);
			else Console.WriteLine(message);
		}

		public void PushIndent(int level) => Indents.Push(level);
		public void PopIndent() => Indents.Pop();
	}
}
