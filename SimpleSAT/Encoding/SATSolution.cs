using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Encoding;

/// <summary>
/// This class parses the solution output of a SAT solver.
/// </summary>
public class SATSolution {
    #region fields
    public SATFormat Format { get; private set; }
    public ulong Cost { get; private set; }
    public Status Solution { get; private set; }
    // Literal values, 0-indexed
    public bool[] Assignments { get; private set; }

    public enum Status {
        Unsatisfiable,
        Unknown,
        OptimumFound,
        SolverFailed
    }
    #endregion

    /// <summary>
    /// Parses the solution of a SAT solver.
    /// </summary>
    /// <param name="solverOutput">The process STDOUT of the SAT solver.</param>
    public SATSolution(SATFormat format, string solverOutput) {
        ParseLines(solverOutput, out string solution, out string valuesRow);
        Solution = solution switch {
            "OPTIMUM FOUND" => Status.OptimumFound,
            "UNSATISFIABLE" => Status.Unsatisfiable,
            _ => Status.Unknown
        };

        if (Solution != Status.OptimumFound) {
            Assignments = new bool[0];
            return;
        }

        Assignments = new bool[valuesRow.Length];

        for (int i = 0; i < valuesRow.Length; i++) {
            bool a = valuesRow[i] == '1';
            Assignments[i] = a;
        }
    }

    private void ParseLines(string solverOutput, out string solution, out string assignments) {
        solution = "";
        assignments = "";

        foreach (string line in solverOutput.Split('\n')) {
            if (line.Length == 0) {
                continue;
            }

            if (line[0] == 's') {
                solution = line.Substring(2);
                continue;
            }
            if (line[0] == 'v') {
                assignments = line.Substring(2);
                continue;
            }
            if (Format == SATFormat.WCNF_MAXSAT && line[0] == 'o') {
                Cost = ulong.Parse(line.Substring(2));
                continue;
            }
        }

        if (solverOutput.Length == 0 || solution.Length == 0) {
            throw new Exception("Invalid solution");
        }
    }

    /// <summary>
    /// Converts the solution to the solution, cost and 
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"s {Solution}");
        if (Format == SATFormat.WCNF_MAXSAT) {
            sb.AppendLine($"o {Cost}");
        }
        sb.Append("v ");
        foreach (bool b in Assignments) {
            sb.Append(b ? '1' : '0');
        }
        return sb.ToString();
    }
}
