using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using static TCLDecompiler.Predicates;

namespace TCLDecompiler {

	public struct Reader<T> where T : unmanaged, IEquatable<T> {
		public static T Invalid;
		private readonly ReadOnlyMemory<T> _buffer;
		private int _position;

		public Reader(in ReadOnlyMemory<T> source) {
			_buffer = source;
			_position = 0;
		}

		public Reader<T> this[Range range] => new(_buffer[range]);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Advance(int count = 1) {
			if (count <= 0 || count + _position > _buffer.Length) return 0;
			_position += count;
			return count;
		}

		public readonly int Remaining {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer.Length - _position;
		}

		public readonly bool IsAtEnd {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _position >= _buffer.Length;
		}

		public int Position {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _position;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => _position = value;
		}

		public readonly bool HasCurrent {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _position < _buffer.Length;
		}
		public readonly ref readonly T Current {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref HasCurrent ? ref _buffer.Span[_position] : ref Invalid;
		}

		public readonly bool HasNext {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _position < _buffer.Length - 1;
		}
		public readonly ref readonly T Next {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => ref HasNext ? ref _buffer.Span[_position + 1] : ref Invalid;
		}

		public readonly ReadOnlySpan<T> UnreadSpan {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer.Span[_position..];
		}

		public readonly ReadOnlySpan<T> ConsumedSpan {
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _buffer.Span[.._position];
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsNext(T ch) => ch.Equals(Current);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsNext(Pred<T> pred) => pred(Current);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsNext(SpanPred<T> pred) => pred(UnreadSpan);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsNextAny(ReadOnlySpan<T> candidates) => candidates.Contains(Current);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsNextAny(params T[] candidates) => IsNextAny(candidates.AsSpan());
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool IsNextExactly(ReadOnlySpan<T> seq) => _position + seq.Length <= _buffer.Length && UnreadSpan[..seq.Length].SequenceEqual(seq);


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T ReadOne() {
			T c = Current;
			Advance();
			return c;
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<T> Read(int amount) {
			if (amount <= 0) return default;
			if (Remaining < amount) throw new ArgumentOutOfRangeException(nameof(amount));

			ReadOnlySpan<T> span = UnreadSpan[..amount];
			Advance(amount);
			return span;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceWhile(Pred<T> pred) => Advance(NextIndexOf(pred));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceWhile(PredNext<T> pred) => Advance(NextIndexOf(pred));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceWhile(PredCount<T> pred) => Advance(NextIndexOf(pred));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceUntil(PredNext<T> pred) => Advance(NextIndexOf(Not(pred)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceUntil(Pred<T> pred) => Advance(NextIndexOf(Not(pred)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceUntil(PredCount<T> pred) => Advance(NextIndexOf(Not(pred)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceUntil(T c) => Advance(NextIndexOf(c));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceIf(T ch) => ch.Equals(Current) ? Advance(1) : 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int AdvanceWhile(T ch) => Advance(NextIndexOf(c => ch.Equals(c)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadWhile(Pred<T> pred) => Read(NextIndexOf(pred));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUntil(Pred<T> pred) => Read(NextIndexOf(Not(pred)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadWhile(PredNext<T> pred) => Read(NextIndexOf(pred));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUntil(PredNext<T> pred) => Read(NextIndexOf(Not(pred)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadWhile(PredCount<T> pred) => Read(NextIndexOf(pred));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUntil(PredCount<T> pred) => Read(NextIndexOf(Not(pred)));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUpTo(T c) => Read(NextIndexOf(c));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUpToAny(params T[] ch) => Read(NextIndexOfAny(ch));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUpToAny(ReadOnlySpan<T> ch) => Read(NextIndexOfAny(ch));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadPast(T c) => Read(NextIndexAfter(c));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public ReadOnlySpan<T> ReadUpTo(ReadOnlySpan<T> c) => Read(NextIndexOf(c));
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryReadPast(ReadOnlySpan<T> c, out ReadOnlySpan<T> match) => (match = Read(NextIndexAfter(c))).Length > 0;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryRead(T ch) => ch.Equals(Current) && Advance() == 1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryRead(Pred<T> pred) => pred(Current) && Advance() == 1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryReadAny(ReadOnlySpan<T> seq) => seq.Contains(Current) && Advance() == 1;
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public bool TryMatch(ReadOnlySpan<T> seq) => IsNextExactly(seq) && Advance(seq.Length) > 0;


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int NextIndexOfAny(ReadOnlySpan<T> seq) {
			T[] arr = seq.ToArray();
			return NextIndexOf(ch => Array.FindIndex(arr, 0, arr.Length, c => c.Equals(ch)) != -1);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int NextIndexOf(T ch) => UnreadSpan.IndexOf(ch);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public int NextIndexOf(ReadOnlySpan<T> delimiter) => UnreadSpan.IndexOf(delimiter);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int NextIndexAfter(T ch) {
			int index = UnreadSpan.IndexOf(ch);
			return index >= 0 ? index + 1 : -1;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int NextIndexAfter(ReadOnlySpan<T> delimiter) {
			int index = UnreadSpan.IndexOf(delimiter);
			return index >= 0 ? index + delimiter.Length : -1;
		}

		public int NextIndexOf(Pred<T> iterator) {
			ReadOnlySpan<T> buff = UnreadSpan;
			int limit = buff.Length;
			int index;
			for (index = 0; index < limit; index++) {
				if (!iterator(buff[index]))
					break;
			}
			return index;
		}

		public int NextIndexOf(SpanPred<T> iterator) {
			ReadOnlySpan<T> buff = UnreadSpan;
			int limit = buff.Length;
			int index;
			for (index = 0; index < limit; index++) {
				if (!iterator(buff[index..]))
					break;
			}
			return index;
		}

		public int NextIndexOf(PredNext<T> iterator) {
			ReadOnlySpan<T> buff = UnreadSpan;
			int limit = buff.Length;
			int index;
			for (index = 0; index < limit; index++) {
				(T cur, T next) = (buff[index], index < limit - 1 ? buff[index + 1] : Invalid);
				if (!iterator(cur, next))
					break;
			}
			return index;
		}

		public int NextIndexOf(PredCount<T> iterator) {
			ReadOnlySpan<T> buff = UnreadSpan;
			int limit = buff.Length;
			int index;
			for (index = 0; index < limit; index++) {
				(T cur, T next) = (buff[index], index < limit - 1 ? buff[index + 1] : Invalid);
				if (!iterator(cur, next, index))
					break;
			}
			return index;
		}

		public int NextIndexOf(PredRemaining<T> iterator) {
			ReadOnlySpan<T> buff = UnreadSpan;
			int limit = buff.Length;
			int index;
			for (index = 0; index < limit; index++) {
				(T cur, T next) = (buff[index], index < limit - 1 ? buff[index + 1] : Invalid);
				if (!iterator(cur, next, index, limit - index))
					break;
			}
			return index;
		}
	}
}
