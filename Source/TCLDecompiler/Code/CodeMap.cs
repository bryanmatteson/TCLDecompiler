using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace TCLDecompiler {
	public class CodeMap {
		private readonly SortedList<int, ICodeUnit> _code;
		private readonly RangeTree<int> _locations;
		private readonly MultiValueDictionary<int, int> _sources;

		public CodeRange Range { get; private set; }
		public bool IsSequential { get; private set; }

		public int Count => _code.Count;
		public IList<int> Locations => _code.Keys;
		public IList<ICodeUnit> Units => _code.Values;
		public ICodeUnit EntryPoint => Units[0];

		public bool ContainsLocation(int location) => _code.ContainsKey(location);
		public bool ContainsIndex(int index) => index >= 0 && index < Count;
		public ICodeUnit CodeAtLocation(int location) => _code[location];

		public CodeMap() {
			_code = new SortedList<int, ICodeUnit>();
			_locations = new RangeTree<int>();
			_sources = new MultiValueDictionary<int, int>();
			Range = CodeRange.Empty;
			IsSequential = false;
		}

		public CodeMap(IEnumerable<Instruction> instructions) : this(instructions.Cast<ICodeUnit>()) { }

		public CodeMap(IEnumerable<ICodeUnit> units) {
			if (units == null) throw new ArgumentNullException(nameof(units));
			_code = new SortedList<int, ICodeUnit>(units.ToDictionary(x => x.Location));
			_locations = new RangeTree<int>();
			_sources = new MultiValueDictionary<int, int>();

			if (_code.Count == 0) {
				Range = CodeRange.Empty;
				IsSequential = false;
			}
			else {
				foreach (ICodeUnit unit in units) {
					_locations.Insert(unit.Location);
					foreach (BranchTarget target in unit.Targets) {
						_sources.Add(target.Location, unit.Location);
					}
				}

				Range = CodeRange.FromBounds(_locations.Min(), _code[_locations.Max()].Range.End);
				IsSequential = CheckIsSequential();
			}
		}

		private bool CheckIsSequential() {
			int min = int.MaxValue, max = int.MinValue, size = 0;

			foreach (ICodeUnit unit in Units) {
				min = Math.Min(min, unit.Location);
				max = Math.Max(max, unit.Location + unit.Size);
				size += unit.Size;
			}

			return max - min == size;
		}

		public void Merge(ICodeUnit unit) {
			var unitsInRange = CodeInRange(unit.Range).ToList();
			if (unitsInRange.Count > 0) {
				if (unit.Range.Start != unitsInRange[0].Location || unit.Range.End != unitsInRange[^1].Range.End)
					throw new ArgumentException("out of range");

				foreach (ICodeUnit u in unitsInRange) {
					_code.Remove(u.Location);
					_locations.Delete(u.Location);
					foreach (BranchTarget target in u.Targets) {
						_sources.Remove(target.Location, u.Location);
					}
				}
			}

			Add(unit);
		}

		public void MergeRange(IEnumerable<ICodeUnit> units) {
			foreach (ICodeUnit unit in units)
				Merge(unit);
		}

		public void Add(ICodeUnit unit) {
			if (_code.ContainsKey(unit.Location)) throw new ArgumentException("range conflicts with already existing code");

			_code[unit.Location] = unit;
			_locations.Insert(unit.Location);
			foreach (BranchTarget target in unit.Targets) {
				_sources.Add(target.Location, unit.Location);
			}

			IsSequential = IsSequential && (unit.Location == Range.End || unit.Range.End == Range.Start);
			Range = CodeRange.FromBounds(_locations.Min(), _code[_locations.Max()].Range.End);
		}

		public void AddRange(IEnumerable<ICodeUnit> units) {
			var newUnits = units.ToList();

			foreach (ICodeUnit unit in newUnits) {
				if (_code.ContainsKey(unit.Location))
					throw new ArgumentException("range conflicts with already existing code");
			}

			foreach (ICodeUnit unit in newUnits) {
				_code[unit.Location] = unit;
				_locations.Insert(unit.Location);
				foreach (BranchTarget target in unit.Targets) {
					_sources.Add(target.Location, unit.Location);
				}
			}

			IsSequential = CheckIsSequential();
			Range = CodeRange.FromBounds(_locations.Min(), _code[_locations.Max()].Range.End);
		}

		public bool Remove(ICodeUnit unit) {
			if (!_code.TryGetValue(unit.Location, out ICodeUnit? u)) return false;
			if (!u.Equals(unit)) return false;

			_code.Remove(unit.Location);
			_locations.Delete(unit.Location);
			foreach (BranchTarget target in unit.Targets) {
				_sources.Remove(target.Location, unit.Location);
			}

			IsSequential = IsSequential && (unit.Range.End == Range.End || unit.Range.Start == Range.Start);
			Range = CodeRange.FromBounds(_locations.Min(), _code[_locations.Max()].Range.End);

			return true;
		}

		public bool Remove(int location) {
			if (!_code.TryGetValue(location, out ICodeUnit? u)) return false;
			_locations.Delete(u.Location);
			foreach (BranchTarget target in u.Targets) {
				_sources.Remove(target.Location, u.Location);
			}

			Range = CodeRange.FromBounds(_locations.Min(), _code[_locations.Max()].Range.End);
			return _code.Remove(u.Location);
		}

		public bool RemoveRange(in CodeRange range) => RemoveRange(range.Start, range.End);
		public bool RemoveRange(int start, int end) {
			int count = 0;
			foreach (ICodeUnit unit in CodeInRange(start, end)) {
				_locations.Delete(unit.Location);
				if (_code.Remove(unit.Location)) count++;

				foreach (BranchTarget target in unit.Targets) {
					_sources.Remove(target.Location, unit.Location);
				}
			}

			Range = CodeRange.FromBounds(_locations.Min(), _code[_locations.Max()].Range.End);
			IsSequential = CheckIsSequential();

			return count > 0;
		}

		public bool TryGetUnit(int location, [NotNullWhen(true)] out ICodeUnit? unit) => _code.TryGetValue(location, out unit);
		public bool TryGetUnit<T>(int location, out T? unit) where T : ICodeUnit {
			unit = default;
			if (!TryGetUnit(location, out ICodeUnit? u)) return false;
			if (u is not T codeUnit) return false;
			unit = codeUnit;
			return true;
		}

		public bool IsRangeSelfContained(in CodeRange range) => IsRangeSelfContained(range.Start, range.End);
		public bool IsRangeSelfContained(int start, int length) {
			foreach (int loc in LocationsInRange(start, length)) {
				if (_code[loc].Targets.Any(t => t.Location < start || t.Location > start + length)) return false;
				if (_sources[loc].Any(t => t < start || t >= start + length)) return false;
			}
			return true;
		}

		public int IndexOfCode(ICodeUnit unit) => Locations.IndexOf(unit.Location);
		public int IndexOfLocation(int location) => Locations.IndexOf(location);
		public int LocationAtIndex(int index) => Locations[index];
		public ICodeUnit CodeAtIndex(int index) => Units[index];
		public IEnumerable<ICodeUnit> CodeInIndexRange(int start, int length) {
			for (int i = 0; i < length; i++) {
				yield return Units[start + i];
			}
		}

		public IEnumerable<ICodeUnit> CodeInIndexRange(in Range range) {
			(int Offset, int Length) = range.GetOffsetAndLength(Units.Count);
			return CodeInIndexRange(Offset, Length);
		}

		public IEnumerable<int> LocationsBranchingTo(int location) => _sources[location];
		public IEnumerable<ICodeUnit> CodeBranchingTo(int location) => _sources[location].Select(l => _code[l]);
		public IEnumerable<int> LocationsInRange(in CodeRange range) => LocationsInRange(range.Start, range.Length);
		public IEnumerable<int> LocationsInRange(int start, int length) {
			if (_code.ContainsKey(start + length) && length > 0) length--;
			return _locations.RangeSearch(start, start + length);
		}

		public IEnumerable<ICodeUnit> CodeInRange(in CodeRange range) => LocationsInRange(range).Select(l => _code[l]);
		public IEnumerable<ICodeUnit> CodeInRange(int start, int length) => LocationsInRange(start, length).Select(l => _code[l]);
		public bool HasCodeInRange(in CodeRange range) => LocationsInRange(range).Any();
		public CodeMap Submap(in CodeRange range) => new(CodeInRange(range));

		public IEnumerable<ICodeUnit> FindRoots() {
			var dict = _code.Keys.ToDictionary(x => x, _ => 0);
			foreach (BranchTarget target in Units.SelectMany(u => u.Targets)) dict[target.Location]++;
			return dict.Where(kv => kv.Value == 0).Select(kv => _code[kv.Key]);
		}

		public IEnumerable<ICodeUnit> FindLeafs() => Units.Where(u => !u.Targets.Any());
		public IEnumerable<CodeRange> GetBasicBlockRanges() {
			var blockStarts = new SortedSet<int>();

			foreach (ICodeUnit unit in Units) {
				foreach (BranchTarget target in unit.Targets) {
					if (unit.Range.End != Range.End)
						blockStarts.Add(unit.Range.End);
					blockStarts.Add(target.Location);
				}
			}

			var starts = blockStarts.ToList();

			for (int i = 0; i < starts.Count - 1; i++) {
				yield return CodeRange.FromBounds(starts[i], starts[i + 1]);
			}
		}
	}
}
