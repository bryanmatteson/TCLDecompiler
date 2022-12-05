using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {

	public readonly struct ForeachInfo {
		public readonly int FirstValue;
		public readonly int LoopCounter;
		public readonly int[][] Lists;

		public ForeachInfo(int firstValue, int loopCounter, int[][] lists) {
			FirstValue = firstValue;
			LoopCounter = loopCounter;
			Lists = lists;
		}

		public override bool Equals(object? obj) => obj is ForeachInfo info &&
				   FirstValue == info.FirstValue &&
				   LoopCounter == info.LoopCounter &&
				   EqualityComparer<int[][]>.Default.Equals(Lists, info.Lists);

		public override int GetHashCode() => HashCode.Combine(FirstValue, LoopCounter, Lists);
	}
}
