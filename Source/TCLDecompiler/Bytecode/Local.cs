using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public readonly struct Local: IEquatable<Local>, ISourceRepresentable {
		public readonly string Name;
		public readonly bool HasDefaultValue;
		public readonly Literal DefaultValue;
		public readonly int FrameIndex;
		public readonly LocalFlags FlagMask;

		public Local(string name, bool hasDefaultValue, Literal defaultValue, int frameIndex, int flagMask) {
			Name = name;
			HasDefaultValue = hasDefaultValue;
			DefaultValue = defaultValue;
			FrameIndex = frameIndex;
			FlagMask = (LocalFlags)flagMask;
		}

		public override bool Equals(object? obj) => obj is Local local && Equals(local);
		public bool Equals(Local local) => Name == local.Name &&
				   HasDefaultValue == local.HasDefaultValue &&
				   EqualityComparer<Literal>.Default.Equals(DefaultValue, local.DefaultValue) &&
				   FrameIndex == local.FrameIndex &&
				   FlagMask == local.FlagMask;

		public override int GetHashCode() => HashCode.Combine(Name, HasDefaultValue, DefaultValue, FrameIndex, FlagMask);
		public string ToSourceString() => Name;
		public override string ToString() => ToSourceString();
	}
}
