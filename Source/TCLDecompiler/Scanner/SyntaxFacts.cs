using System;
using System.Runtime.CompilerServices;

namespace TCLDecompiler {
	public static partial class SyntaxFacts {
		public static readonly char InvalidChar = Chars.InvalidChar;

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsTerminal(char cur) => IsWhitespace(cur) || IsNewline(cur) || cur == Chars.CloseBrace || cur == Chars.CloseBrace || cur == Chars.CloseParen || cur == InvalidChar;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsExponentSign(char cur) => cur == 'e' || cur == 'E';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsPlusOrMinus(char cur) => cur == '+' || cur == '-';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsComment(char cur, char next) => cur == '#' || (cur == '/' && (next == '/' || next == '*'));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsLetter(char ch) => IsLowerAsciiLetter(ch) || IsUpperAsciiLetter(ch) || (ch >= 0x80 && char.IsLetter(ch));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsLetterOrDigit(char ch) => IsLetter(ch) || IsDigit(ch);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsLowerAsciiLetter(char ch) => ch >= 'a' && ch <= 'z';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsUpperAsciiLetter(char ch) => ch >= 'A' && ch <= 'Z';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsSingleEscapeCharacter(char ch) => ch == 'a' || ch == 'b' || ch == 'f' || ch == 'n' || ch == 'r' || ch == 't' || ch == 'v' || ch == Chars.DoubleQuote || ch == Chars.Backslash;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsIdentifierPart(char ch) => IsLetterOrUnderscore(ch) || IsDigit(ch) || ch == '.' || ch == '-';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsFirstIdentifierPart(char ch) => IsLowerAsciiLetter(ch) || IsUpperAsciiLetter(ch) || ch == '_';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsDigit(char ch) => (ch >= '0' && ch <= '9') || char.IsDigit(ch);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsOctalDigit(char ch) => ch >= '0' && ch <= '7';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsBinaryDigit(char ch) => ch >= '0' && ch <= '1';
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsHexDigit(char ch) => (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsLetterOrUnderscore(char ch) => IsLowerAsciiLetter(ch) || IsUpperAsciiLetter(ch) || ch == '_' || (ch > 0x7F && char.IsLetter(ch));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNewline(char ch) => ch == Chars.LineFeed || ch == Chars.CarriageReturn || ch == Chars.LineSeparator || ch == Chars.ParagraphSeparator || ch == Chars.NextLine;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsNewlineWithoutCarriageReturn(char ch) => ch == Chars.LineFeed || ch == Chars.LineSeparator || ch == Chars.ParagraphSeparator || ch == Chars.NextLine;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool IsWhitespaceOrNewline(char ch) => IsNewline(ch) || IsWhitespace(ch);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsWhitespace(char ch)
			=> ch == Chars.Space || ch == Chars.CarriageReturn || ch == Chars.LineFeed
			|| ch == Chars.EmQuad || ch == Chars.EmSpace || ch == Chars.FourPerEmSpace
			|| ch == Chars.SixPerEmSpace || ch == Chars.PunctuationSpace || ch == Chars.ThinSpace
			|| ch == Chars.ZeroWidthSpace || ch == Chars.IdeographicSpace || ch == Chars.MathematicalSpace
			|| ch == Chars.Tab || ch == Chars.NonBreakingSpace
			|| ch == Chars.ThreePerEmSpace || ch == Chars.HairSpace || ch == Chars.EnSpace || ch == Chars.FigureSpace
			|| ch == Chars.NarrowNoBreakSpace || ch == Chars.Ogham;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int DigitVal(char ch) {
			if ('0' <= ch && ch <= '9') return ch - '0';
			if ('a' <= ch && ch <= 'f') return ch - 'a' + 10;
			if ('A' <= ch && ch <= 'F') return ch - 'A' + 10;
			return 16;
		}
	}
}
