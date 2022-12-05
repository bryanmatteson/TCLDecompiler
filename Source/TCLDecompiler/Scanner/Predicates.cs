using System;
using System.Runtime.CompilerServices;

namespace TCLDecompiler {
	public delegate bool Pred<in T>(T current);
	public delegate bool SpanPred<T>(ReadOnlySpan<T> current);

	public delegate bool PredNext<in T>(T current, T next);
	public delegate bool PredCount<in T>(T current, T next, int count);
	public delegate bool PredRemaining<in T>(T current, T next, int count, int remaining);

	public static class Predicates {
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Pred<T> Not<T>(Pred<T> iterator) => item => !iterator(item);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Pred<T> Or<T>(Pred<T> lhs, Pred<T> rhs) => item => lhs(item) || rhs(item);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static Pred<T> And<T>(Pred<T> lhs, Pred<T> rhs) => item => lhs(item) && rhs(item);


		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static SpanPred<T> Not<T>(SpanPred<T> iterator) => item => !iterator(item);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static SpanPred<T> Or<T>(SpanPred<T> lhs, SpanPred<T> rhs) => item => lhs(item) || rhs(item);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static SpanPred<T> And<T>(SpanPred<T> lhs, SpanPred<T> rhs) => item => lhs(item) && rhs(item);


		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredNext<T> Not<T>(PredNext<T> iterator) => (cur, next) => !iterator(cur, next);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredNext<T> Or<T>(PredNext<T> lhs, PredNext<T> rhs) => (cur, next) => lhs(cur, next) || rhs(cur, next);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredNext<T> And<T>(PredNext<T> lhs, PredNext<T> rhs) => (cur, next) => lhs(cur, next) && rhs(cur, next);



		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredCount<T> Not<T>(PredCount<T> iterator) => (cur, next, count) => !iterator(cur, next, count);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredCount<T> Or<T>(PredCount<T> lhs, PredCount<T> rhs) => (cur, next, count) => lhs(cur, next, count) || rhs(cur, next, count);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredCount<T> And<T>(PredCount<T> lhs, PredCount<T> rhs) => (cur, next, count) => lhs(cur, next, count) && rhs(cur, next, count);


		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredRemaining<T> Not<T>(PredRemaining<T> iterator) => (cur, next, count, rem) => !iterator(cur, next, count, rem);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredRemaining<T> Or<T>(PredRemaining<T> lhs, PredRemaining<T> rhs) => (cur, next, count, rem) => lhs(cur, next, count, rem) || rhs(cur, next, count, rem);
		[MethodImpl(MethodImplOptions.AggressiveInlining)] public static PredRemaining<T> And<T>(PredRemaining<T> lhs, PredRemaining<T> rhs) => (cur, next, count, rem) => lhs(cur, next, count, rem) && rhs(cur, next, count, rem);
	}
}
