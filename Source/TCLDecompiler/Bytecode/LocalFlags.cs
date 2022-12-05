using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {

	[Flags]
	public enum LocalFlags: int {
		Scalar = 0,
		Array = 1 << 0,
		Link = 1 << 1,
		InHashtable = 1 << 2,
		DeadHash = 1 << 3,
		TracedRead = 1 << 4,
		TracedWrite = 1 << 5,
		TracedUnset = 1 << 6,
		NamespaceVar = 1 << 7,
		Argument = 1 << 8,
		Temporary = 1 << 9,
		IsArgs = 1 << 10,
		TracedArray = 1 << 11,
		AllTraces = TracedRead | TracedWrite | TracedArray | TracedUnset,
		ArrayElement = 1 << 12,
		AllHash = InHashtable | DeadHash | NamespaceVar | ArrayElement,
		TraceActive = 1 << 13,
		SearchActive = 1 << 14,
		Resolved = 1 << 15,
	}
}
