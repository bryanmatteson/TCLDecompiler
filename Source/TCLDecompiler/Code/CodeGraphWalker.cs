using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace TCLDecompiler {
	public abstract class CodeGraphWalker {
		protected bool AbortWalk { private get; set; }
		private ICodeUnit? _lastUnit;

		protected void Walk(CodeMap codeMap) {
			_lastUnit = codeMap.EntryPoint;
			Visit(codeMap, codeMap.EntryPoint, new HashSet<int>());
			OnEnd(codeMap);
		}

		protected void Walk(CodeMap codeMap, ICodeUnit unit) {
			_lastUnit = unit;
			Visit(codeMap, unit, new HashSet<int>());
			OnEnd(codeMap);
		}

		private void Visit(CodeMap codeMap, ICodeUnit unit, HashSet<int> visitedLocations) {
			if (AbortWalk) return;

			visitedLocations.Add(unit.Location);
			OnVisiting(codeMap, unit);
			_lastUnit = unit;

			foreach (BranchTarget target in unit.Targets.Where(t => t.Type == BranchType.Conditional)) {
				if (codeMap.TryGetUnit(target.Location, out ICodeUnit? branchTo)) {
					if (!visitedLocations.Contains(branchTo.Location) || OnCycleDetected(codeMap, unit, branchTo)) {
						OnBeginBranch(codeMap, unit, branchTo);
						Visit(codeMap, branchTo, visitedLocations);
						OnEndBranch(codeMap, _lastUnit, unit);
					}
				}
			}

			foreach (BranchTarget target in unit.Targets.Where(t => t.Type != BranchType.Conditional && t.Type != BranchType.None)) {
				if (codeMap.TryGetUnit(target.Location, out ICodeUnit? nextUnit)) {
					if (!visitedLocations.Contains(nextUnit.Location) || OnCycleDetected(codeMap, unit, nextUnit)) {
						Visit(codeMap, nextUnit, visitedLocations);
					}
				}
			}
		}

		protected abstract void OnVisiting(CodeMap codeMap, ICodeUnit inst);
		protected virtual bool OnCycleDetected(CodeMap codeMap, ICodeUnit pc, ICodeUnit alreadyVisited) => false;
		protected virtual void OnBeginBranch(CodeMap codeMap, ICodeUnit pc, ICodeUnit branchTo) { }
		protected virtual void OnEndBranch(CodeMap codeMap, ICodeUnit pc, ICodeUnit origin) { }
		protected virtual void OnEnd(CodeMap codeMap) { }
	}
}
