using SimpleSAT.Encoding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

public static class SATSolver {
    /// <summary>
    /// Solve a SAT encoding with a given SAT solver. The solver only accepts the new solution line format.
    /// </summary>
    /// <param name="solverBinaryPath">The path to the binary of the SAT solver</param>
    /// <param name="inputCnfFile">The input CNF file to use</param>
    /// <param name="format">The format of the CNF file</param>
    /// <param name="timeLimitSeconds">Solver process time limit in seconds</param>
    /// <param name="additionalArguments">Additional flags to pass to the solver</param>
    /// <returns></returns>
    public static SolverResult Solve(string solverBinaryPath, string inputCnfFile, SATFormat format, int timeLimitSeconds = 0, string? additionalArguments = "") {
        return SolveWithProcess(GetSolverProcess(solverBinaryPath, inputCnfFile, additionalArguments), format, timeLimitSeconds);
    }

    /// <summary>
    /// Solve a SAT encoding with a given SAT solver process. The solver only accepts the new solution line format.
    /// </summary>
    /// <param name="solverProcess">The process to start which launches the SAT solver and solves a CNF file</param>
    /// <param name="format">The format of the CNF file</param>
    /// <param name="timeLimitSeconds">Solver process time limit in seconds</param>
    /// <returns></returns>
    public static SolverResult SolveWithProcess(Process solverProcess, SATFormat format, int timeLimitSeconds = 0) {
        if (!solverProcess.StartInfo.RedirectStandardOutput) {
            throw new Exception("The process to use must redirect the standard output");
        }

        string? solverOutput = BenchProcess(solverProcess, timeLimitSeconds, out ulong solveTimeMs, out bool graceful);
        if (!graceful) {
            return new SolverResult(SolverResult.ProcessStatus.SolverTimeout, solveTimeMs, graceful, null);
        }

        if (solverOutput != null) {
            try {
                SATSolution solution = new SATSolution(format, solverOutput);
                return new SolverResult(SolverResult.ProcessStatus.Succes, solveTimeMs, true, solution);
            } catch (Exception) { }
        }

        return new SolverResult(SolverResult.ProcessStatus.ErrorParsingOutput, solveTimeMs, true, null);
    }

    private static string? BenchProcess(Process p, int timeLimitSeconds, out ulong elapsedTime, out bool gracefulExit) {
        Stopwatch watch = Stopwatch.StartNew();
        p.Start();

        if (timeLimitSeconds > 0) {
            gracefulExit = p.WaitForExit(timeLimitSeconds * 1000);
            watch.Stop();
            if (!gracefulExit) {
                p.Kill();
            }
        } else {
            p.WaitForExit();
            watch.Stop();
            gracefulExit = true;
        }

        elapsedTime = (ulong)watch.ElapsedMilliseconds;

        if (!gracefulExit || p.ExitCode != 0) {
            return null;
        }

        return p.StandardOutput.ReadToEnd();
    }

    private static Process GetSolverProcess(string solverBinaryPath, string inputCnfFile, string? arguments) {
        Process solverProcess = new Process();
        solverProcess.StartInfo.FileName = solverBinaryPath;
        solverProcess.StartInfo.Arguments = $"{inputCnfFile} {arguments}";
        solverProcess.StartInfo.RedirectStandardOutput = true;
        return solverProcess;
    }
}

public struct SolverResult {
    /// <summary>
    /// Status regarding what happened with the process being run
    /// </summary>
    public ProcessStatus Status { get; }
    /// <summary>
    /// Solve time in milliseconds
    /// </summary>
    public ulong SolveTimeMs { get; }
    /// <summary>
    /// Did the solver complete succesfully (false if time limit exceeded or solver error e.g. memory out)
    /// </summary>
    public bool SolverCompleted { get; }
    /// <summary>
    /// The SAT solution
    /// </summary>
    public SATSolution? Solution { get; }

    public enum ProcessStatus {
        SolverTimeout,
        ErrorParsingOutput,
        Succes
    }

    public SolverResult(ProcessStatus status, ulong solveTimeMs, bool solverCompleted, SATSolution? solution) {
        Status = status;
        SolveTimeMs = solveTimeMs;
        SolverCompleted = solverCompleted;
        Solution = solution;
    }
}