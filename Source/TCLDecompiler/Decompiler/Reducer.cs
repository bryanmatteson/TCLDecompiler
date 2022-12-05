using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static TCLDecompiler.CommandFormatters;

namespace TCLDecompiler {
	public static class LogicalsReducer {
		public static void Reduce(CodeMap codeMap) {
			var jumps = codeMap.Units.OfType<Instruction>().Where(i => i.Opcode.IsBranch()).ToList();
			foreach (var (idx, jmp) in jumps.Select((j, i) => (i, j))) {
				if ((idx + 2) >= jumps.Count) continue;
				if (!jumps[idx].Opcode.IsConditionalBranch() || !jumps[idx + 1].Opcode.IsUnconditionalBranch() || !jumps[idx + 2].Opcode.IsConditionalBranch()) continue;
				var dupLocation = jumps[idx + 1].Targets.First().Location;
				if (!codeMap.ContainsLocation(dupLocation)) continue;
				var dup = codeMap.CodeAtLocation(dupLocation) as Instruction?;
				if (dup is null) continue;
				if (!(dup?.Targets.First().Location == jumps[idx + 2].Location)) continue;
				var testCmd = codeMap.CodeBranchingTo(jumps[idx].Location).FirstOrDefault() as Command;
				if (testCmd is null) continue;
				var codes = codeMap.CodeInRange(CodeRange.FromBounds(testCmd.Range.End, jumps[idx + 2].Range.End));
				testCmd.Merge(codes.ToArray());
				codeMap.Merge(testCmd);
				InstructionReducer.Reduce(codeMap);
			}
		}
	}
	public static class CatchReducer {
		private static IEnumerable<(int start, int end)> Stitched(IEnumerable<int> startLocations, IEnumerable<int> endLocations) {
			var starts = startLocations.ToHashSet();
			var ends = endLocations.ToHashSet();

			int count = 0, start = 0;
			foreach (int loc in new SortedSet<int>(starts.Concat(ends))) {
				if (starts.Contains(loc)) {
					if (count == 0) start = loc;
					count++;
				}
				if (ends.Contains(loc)) count--;
				if (count == 0) yield return (start, loc);
			}
		}

		public static void Reduce(CodeMap codeMap) {
			IEnumerable<Instruction> insts = codeMap.Units.OfType<Instruction>();
			IEnumerable<int> starts = insts.Where(i => i.Opcode == Opcode.BeginCatch4).Select(i => i.Location);
			IEnumerable<int> ends = insts.Where(i => i.Opcode == Opcode.EndCatch).Select(i => i.Location + i.Size);

			foreach ((int start, int end) in Stitched(starts, ends).Reverse()) {
				var range = CodeRange.FromBounds(start, end);
				var units = codeMap.CodeInRange(range).ToList();
				var jumps = units.Where(u => u is Instruction inst && inst.Opcode.IsUnconditionalBranch()).ToList();
				if (jumps.Count != 1) continue;

				var codeRange = CodeRange.FromBounds(units[0].Range.End, jumps[0].Location);
				var catchCodes = codeMap.CodeInRange(codeRange).ToList();
				if (catchCodes.Count > 0 && catchCodes[^1] is ICommand cmd && cmd.ToSourceString() == "0") {
					catchCodes.RemoveAt(catchCodes.Count - 1);
				}

				codeMap.Merge(new Command(units, Catch(catchCodes.ToArray())));
			}
			InstructionReducer.Reduce(codeMap);
		}
	}

	public static class InstructionReducer {
		public static void Reduce(CodeMap codeMap) {
			while (TryReduce(codeMap, out IList<ICommand> commands)) {
				codeMap.MergeRange(commands);
			}
		}

		private static bool TryReduce(CodeMap codeMap, out IList<ICommand> commands) {
			commands = new List<ICommand>();

			foreach (ICodeUnit unit in codeMap.Units.ToList()) {
				if (unit is not Instruction inst) continue;
				int index = codeMap.IndexOfCode(unit);
				if (inst.ArgumentCount > index || inst.ArgumentCount < 0) continue;
				ICodeUnit[] args = codeMap.CodeInIndexRange((index - inst.ArgumentCount)..index).ToArray();
				if (!args.All(a => a is ICommand)) continue;
				IFormattable formatter;

				switch (inst.Opcode) {
					case Opcode.Push1:
					case Opcode.Push4:
						formatter = Lit(inst.Operands[0]);
						break;

					case Opcode.InvokeStk1:
					case Opcode.InvokeStk4:
						formatter = Cmd(args);
						break;

					case Opcode.LoadScalar1:
					case Opcode.LoadScalar4:
						formatter = Var(inst.Operands[0]);
						break;

					case Opcode.LoadScalarStk:
						formatter = Cmd("set", args[0]);
						break;

					case Opcode.LoadArray1:
					case Opcode.LoadArray4:
						formatter = Var(inst.Operands[0]);
						break;

					case Opcode.LoadArrayStk:
						formatter = Var(args[0]);
						break;

					case Opcode.StoreScalar1:
					case Opcode.StoreScalar4:
						formatter = Cmd("set", inst.Operands[0], args[0]);
						break;

					case Opcode.StoreStk:
					case Opcode.StoreScalarStk:
						formatter = Cmd("set", args);
						break;

					case Opcode.StoreArray1:
					case Opcode.StoreArray4:
						formatter = Cmd("set", Arr(inst.Operands[0], args[0]), args[1]);
						break;

					case Opcode.StoreArrayStk:
						formatter = Cmd("set", Arr(args[0], args[1]), args[2]);
						break;

					case Opcode.Lor:
					case Opcode.Land:
					case Opcode.Bitor:
					case Opcode.Bitxor:
					case Opcode.Bitand:
					case Opcode.Eq:
					case Opcode.Neq:
					case Opcode.Lt:
					case Opcode.Gt:
					case Opcode.Le:
					case Opcode.Ge:
					case Opcode.Lshift:
					case Opcode.Rshift:
					case Opcode.Add:
					case Opcode.Sub:
					case Opcode.Mult:
					case Opcode.Div:
					case Opcode.Mod:
					case Opcode.Uplus:
					case Opcode.Uminus:
					case Opcode.Bitnot:
					case Opcode.Not:
						formatter = Op(inst.Opcode, args);
						break;

					case Opcode.CallBuiltinFunc1:
						formatter = Cmd(((BuiltInMathFunction)inst.Operands[0]).GetFunctionName(), args);
						break;

					case Opcode.Break:
						formatter = Lit("break");
						break;

					case Opcode.Continue:
						formatter = Lit("continue");
						break;

					case Opcode.StrConcat1:
						formatter = QLit(Cat(string.Empty, args));
						break;

					case Opcode.ExprStk:
						formatter = Cmd("expr", Body(args));
						break;

					case Opcode.LoadStk:
						formatter = Var(args[0]);
						break;

					case Opcode.StrEq:
						formatter = Cmd("string equal", args);
						break;

					case Opcode.StrNeq:
						formatter = Cmd("!string equal", args);
						break;

					case Opcode.StrCmp:
						formatter = Cmd("string compare", args);
						break;

					case Opcode.StrLen:
						formatter = Cmd("string length", args);
						break;

					case Opcode.StrIndex:
						formatter = Cmd("string index", args);
						break;

					case Opcode.StrMatch:
						formatter = Cmd("string match", args);
						break;

					case Opcode.StrTrim:
						formatter = Cmd("string trim", args);
						break;

					case Opcode.StrTrimLeft:
						formatter = Cmd("string trimleft", args);
						break;

					case Opcode.StrTrimRight:
						formatter = Cmd("string trimright", args);
						break;

					case Opcode.StrUpper:
						formatter = Cmd("string toupper", args);
						break;

					case Opcode.StrLower:
						formatter = Cmd("string tolower", args);
						break;

					case Opcode.StrTitle:
						formatter = Cmd("string totitle", args);
						break;

					case Opcode.StrReplace:
						formatter = Cmd("string replace", args);
						break;

					case Opcode.StrClass:
						formatter = Cmd("string is", args);
						break;

					case Opcode.List:
						formatter = Cmd("list", args);
						break;

					case Opcode.ListIndex:
						formatter = Cmd("lindex", args);
						break;

					case Opcode.ListLength:
						formatter = Cmd("llength", args);
						break;

					case Opcode.AppendArray1:
					case Opcode.AppendArray4:
						formatter = Cmd("append", Arr(inst.Operands[0], args[0]), args[1]);
						break;

					case Opcode.AppendArrayStk:
						formatter = Cmd("append", Arr(args[0], args[1]), args[2]);
						break;

					case Opcode.AppendScalar1:
					case Opcode.AppendScalar4:
						formatter = Cmd("append", inst.Operands[0], args[0]);
						break;

					case Opcode.AppendStk:
						formatter = Cmd("append", args);
						break;

					case Opcode.LappendScalar1:
					case Opcode.LappendScalar4:
						formatter = Cmd("lappend", inst.Operands[0], args[0]);
						break;

					case Opcode.LappendStk:
						formatter = Cmd("lappend", args);
						break;

					case Opcode.LappendArray1:
					case Opcode.LappendArray4:
						formatter = Cmd("lappend", Arr(inst.Operands[0], args[0]), args[1]);
						break;

					case Opcode.LappendArrayStk:
						formatter = Cmd("lappend", Arr(args[0], args[1]), args[2]);
						break;

					case Opcode.LappendListArray:
						formatter = Cmd("lappend", Arr(inst.Operands[0], args[0]), Spaced(args[1..]));
						break;

					case Opcode.LappendListArrayStk:
						formatter = Cmd("lappend", Arr(args[0], args[1]), Spaced(args[2..]));
						break;

					case Opcode.LappendList:
						formatter = Cmd("lappend", inst.Operands[0], Spaced(args));
						break;

					case Opcode.LappendListStk:
						formatter = Cmd("lappend", args);
						break;


					case Opcode.TryCvtToNumeric:
					case Opcode.Done:
					case Opcode.Pop:
						if (codeMap.CodeAtIndex(index - 1) is not ICommand cmd) continue;
						cmd.Append(inst);
						commands.Add(cmd);
						continue;

					case Opcode.IncrScalar1Imm:
						formatter = Cmd("incr", inst.Operands[0], inst.Operands[1]);
						break;

					case Opcode.IncrScalarStkImm:
						formatter = Cmd("incr", args[0], inst.Operands[0]);
						break;

					case Opcode.IncrScalar1:
						formatter = Cmd("incr", inst.Operands[0], args[0]);
						break;

					case Opcode.IncrScalarStk:
						formatter = Cmd("incr", args[0], args[1]);
						break;

					case Opcode.IncrStkImm:
						formatter = Cmd("incr", args[0], inst.Operands[0]);
						break;

					case Opcode.IncrStk:
						formatter = Cmd("incr", args[0], args[1]);
						break;

					case Opcode.IncrArray1Imm:
						formatter = Cmd("incr", Arr(inst.Operands[0], args[0]), inst.Operands[1]);
						break;

					case Opcode.IncrArrayStkImm:
						formatter = Cmd("incr", Arr(args[0], args[1]), inst.Operands[0]);
						break;

					case Opcode.IncrArray1:
						formatter = Cmd("incr", Arr(inst.Operands[0], args[0]), args[1]);
						break;

					case Opcode.IncrArrayStk:
						formatter = Cmd("incr", Arr(args[0], args[1]), args[2]);
						break;

					case Opcode.EvalStk:
					case Opcode.Dup:
					case Opcode.CallFunc1:
					case Opcode.ForeachStart4:
					case Opcode.ForeachStep4:
					case Opcode.BeginCatch4:
					case Opcode.EndCatch:
					case Opcode.PushResult:
					case Opcode.PushReturnCode:
					case Opcode.ListIndexMulti:
					case Opcode.Over:
					case Opcode.LsetList:
					case Opcode.LsetFlat:
					case Opcode.ReturnImm:
					case Opcode.Expon:
					case Opcode.ExpandStart:
					case Opcode.ExpandStktop:
					case Opcode.InvokeExpanded:
					case Opcode.ListIndexImm:
					case Opcode.ListRangeImm:
					case Opcode.StartCmd:
					case Opcode.ListIn:
					case Opcode.ListNotIn:
					case Opcode.PushReturnOptions:
					case Opcode.ReturnStk:
					case Opcode.DictGet:
					case Opcode.DictSet:
					case Opcode.DictUnset:
					case Opcode.DictIncrImm:
					case Opcode.DictAppend:
					case Opcode.DictLappend:
					case Opcode.DictFirst:
					case Opcode.DictNext:
					case Opcode.DictDone:
					case Opcode.DictUpdateStart:
					case Opcode.DictUpdateEnd:
					case Opcode.JumpTable:
					case Opcode.Upvar:
					case Opcode.Nsupvar:
					case Opcode.Variable:
					case Opcode.Syntax:
					case Opcode.Reverse:
					case Opcode.Regexp:
					case Opcode.ExistScalar:
					case Opcode.ExistArray:
					case Opcode.ExistArrayStk:
					case Opcode.ExistStk:
					case Opcode.Nop:
					case Opcode.ReturnCodeBranch:
					case Opcode.UnsetScalar:
					case Opcode.UnsetArray:
					case Opcode.UnsetArrayStk:
					case Opcode.UnsetStk:
					case Opcode.DictExpand:
					case Opcode.DictRecombineStk:
					case Opcode.DictRecombineImm:
					case Opcode.DictExists:
					case Opcode.DictVerify:
					case Opcode.StrMap:
					case Opcode.StrFind:
					case Opcode.StrFindLast:
					case Opcode.StrRangeImm:
					case Opcode.StrRange:
					case Opcode.Yield:
					case Opcode.CoroutineName:
					case Opcode.Tailcall:
					case Opcode.NsCurrent:
					case Opcode.InfoLevelNum:
					case Opcode.InfoLevelArgs:
					case Opcode.ResolveCommand:
					case Opcode.TclooSelf:
					case Opcode.TclooClass:
					case Opcode.TclooNs:
					case Opcode.TclooIsObject:
					case Opcode.ArrayExistsStk:
					case Opcode.ArrayExistsImm:
					case Opcode.ArrayMakeStk:
					case Opcode.ArrayMakeImm:
					case Opcode.InvokeReplace:
					case Opcode.ListConcat:
					case Opcode.ExpandDrop:
					case Opcode.ForeachStart:
					case Opcode.ForeachStep:
					case Opcode.ForeachEnd:
					case Opcode.LmapCollect:
					case Opcode.ConcatStk:
					case Opcode.OriginCommand:
					case Opcode.TclooNext:
					case Opcode.TclooNextClass:
					case Opcode.YieldToInvoke:
					case Opcode.NumType:
					case Opcode.TryCvtToBoolean:
					case Opcode.ClockRead:
					case Opcode.Jump1:
					case Opcode.Jump4:
					case Opcode.JumpTrue1:
					case Opcode.JumpTrue4:
					case Opcode.JumpFalse1:
					case Opcode.JumpFalse4:
						continue;


					default: throw new InvalidOperationException("invalid opcode");
				}

				if (formatter is not null) {
					commands.Add(new Command(args.Append(inst), formatter));
				}
			}

			return commands.Count > 0;
		}
	}
}
