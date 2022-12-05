using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public static class StringExtensions {
		public static string RightJustified(this string str, int width, bool truncate = false) {
			if (width <= str.Length) return truncate ? str.AsSpan(str.Length - width).ToString() : str;
			return new string(' ', width - str.Length) + str;
		}

		public static string LeftJustified(this string str, int width, bool truncate = false) {
			if (width <= str.Length) return truncate ? str.AsSpan(0, width).ToString() : str;
			return str + new string(' ', width - str.Length);
		}

		public static string Truncate(this string str, int maxWidth, string trailing = "…") {
			int maxStrLen = maxWidth - trailing.Length;
			return str.Length > maxStrLen ? str.AsSpan(0, maxStrLen).ToString() + trailing : str;
		}
	}
}
