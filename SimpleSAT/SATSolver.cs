using SimpleSAT.Encoding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

public static class SATSolver {
    /// <summary>
    /// Directory where to save temporary files
    /// </summary>
    public static string WorkingDirectory { get; set; } = "./";

    private static string GetFile(string name) => Path.Combine(WorkingDirectory, name);

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
        return ExecuteProcess(GetSolverProcess(solverBinaryPath, inputCnfFile, additionalArguments), format, timeLimitSeconds, null);
    }

    /// <summary>
    /// Solve a SAT encoding with a given SAT solver. The solver only accepts the new solution line format.
    /// </summary>
    /// <param name="solverBinaryPath">The path to the binary of the SAT solver</param>
    /// <param name="inputCnfFile">The input CNF file to use</param>
    /// <param name="format">The format of the CNF file</param>
    /// <param name="timeLimitSeconds">Solver process time limit in seconds</param>
    /// <param name="additionalArguments">Additional flags to pass to the solver</param>
    /// <returns></returns>
    public static SolverResult SolveWithTimeCommand(string solverBinaryPath, string inputCnfFile, SATFormat format, int timeLimitSeconds = 0, string? additionalArguments = "", string timeBinPath = "/usr/bin/time", string? timeOutputPath = null) {
        timeOutputPath = timeOutputPath ?? $"time_output_{Environment.TickCount64}.out";
        if (File.Exists(GetFile(timeOutputPath))) {
            throw new Exception("Time output file exists: " + timeOutputPath);
        }
        return ExecuteProcess(GetTimeSolverProcess(solverBinaryPath, inputCnfFile, additionalArguments, timeBinPath, timeOutputPath), format, timeLimitSeconds, timeOutputPath);
    }

    /// <summary>
    /// Solve a SAT encoding with a given SAT solver process. The solver only accepts the new solution line format.
    /// </summary>
    /// <param name="solverProcess">The process to start which launches the SAT solver and solves a CNF file</param>
    /// <param name="format">The format of the CNF file</param>
    /// <param name="timeLimitSeconds">Solver process time limit in seconds</param>
    /// <returns></returns>
    public static SolverResult SolveWithProcess(Process solverProcess, SATFormat format, int timeLimitSeconds = 0) {
        return ExecuteProcess(new ProcessContainer(solverProcess), format, timeLimitSeconds, null);
    }

    private static SolverResult ExecuteProcess(ProcessContainer solverProcess, SATFormat format, int timeLimitSeconds, string? timeOutputPath) {
        if (!solverProcess.Process.StartInfo.RedirectStandardOutput) {
            throw new Exception("The process to use must redirect the standard output");
        }

        string? solverOutput = BenchProcess(solverProcess, timeLimitSeconds, out long realTimeMs, out bool graceful);
        string? timeOutput = null;

        Console.WriteLine("Solver output:");
        Console.WriteLine(solverOutput);

        if (timeOutputPath != null && File.Exists(GetFile(timeOutputPath))) {
            timeOutput = File.ReadAllText(GetFile(timeOutputPath));
            File.Delete(GetFile(timeOutputPath));
        }

        Times times = timeOutput == null ? new Times(realTimeMs) : ParseUserTime(timeOutput);

        if (!graceful) {
            return new SolverResult(solverOutput, SolverResult.ProcessStatus.SolverTimeout, new Times(-1), graceful, null);
        }

        if (solverOutput != null) {
            try {
                Console.WriteLine("Parsing solver output...");
                SATSolution solution = new SATSolution(format, solverOutput);
                return new SolverResult(solverOutput, SolverResult.ProcessStatus.Succes, times, true, solution);
            } catch (Exception e) {
                Console.WriteLine("Error parsing solver output: " + e);
            }
        }

        return new SolverResult(solverOutput, SolverResult.ProcessStatus.ErrorParsingOutput, times, true, null);
    }

    private static Times ParseUserTime(string outputTxt) {
        string[] output = outputTxt.Split('\n');

        long real = -1;
        long user = -1;
        long sys = -1;

        foreach (string line in output) {
            if (line.StartsWith("real ")) {
                ParseTime(ref real, line);
            }
            if (line.StartsWith("user ")) {
                ParseTime(ref user, line);
            }
            if (line.StartsWith("sys ")) {
                ParseTime(ref sys, line);
            }
        }

        return new Times(real, user, sys);
    }

    private static void ParseTime(ref long time, string value) {
        string[] parts = value.Split(' ');
        double val = double.Parse(parts[1], CultureInfo.InvariantCulture);
        time = (long)(val * 1000);
    }

    private static string? BenchProcess(ProcessContainer p, int timeLimitSeconds, out long elapsedRealTime, out bool gracefulExit) {
        Stopwatch watch = Stopwatch.StartNew();
        p.Process.Start();

        if (timeLimitSeconds > 0) {
            gracefulExit = p.Process.WaitForExit(timeLimitSeconds * 1000);
            watch.Stop();
            if (!gracefulExit) {
                p.Process.Kill();
            }
        } else {
            p.Process.WaitForExit();
            watch.Stop();
            gracefulExit = true;
        }

        elapsedRealTime = watch.ElapsedMilliseconds;

        // Pacose
        if (!gracefulExit) {// || p.Process.ExitCode != 0) {
            return null;
        }

        string output = p.Process.StandardOutput.ReadToEnd();
        p.Dispose();

        return output;
    }

    private static ProcessContainer GetSolverProcess(string solverBinaryPath, string inputCnfFile, string? arguments) {
        Process solverProcess = new Process();
        solverProcess.StartInfo.FileName = solverBinaryPath;
        solverProcess.StartInfo.Arguments = $"{inputCnfFile} {arguments}";
        solverProcess.StartInfo.RedirectStandardOutput = true;
        return new ProcessContainer(solverProcess);
    }

    private static ProcessContainer GetTimeSolverProcess(string solverBinaryPath, string inputCnfFile, string? arguments, string timeBinPath, string timeOutputPath) {
        if (!File.Exists(solverBinaryPath)) {
            throw new Exception("Could not find solver: " + solverBinaryPath);
        }

        Process solverProcess = new Process();
        solverProcess.StartInfo.FileName = timeBinPath;
        solverProcess.StartInfo.Arguments = $"-p -o {GetFile(timeOutputPath)} {solverBinaryPath} {inputCnfFile} {arguments}";
        Console.WriteLine("Args: " + solverProcess.StartInfo.Arguments);
        solverProcess.StartInfo.RedirectStandardOutput = true;
        return new ProcessContainer(solverProcess);
    }
}

internal class ProcessContainer : IDisposable {
    #region fields
    public Process Process { get; }
    public string? BashInstructionFile { get; }
    #endregion

    public ProcessContainer(Process process, string? file = null) {
        Process = process;
        BashInstructionFile = file;
    }

    public void Dispose() {
        Process.Dispose();
        if (BashInstructionFile != null) {
            File.Delete(BashInstructionFile);
        }
    }
}

public struct SolverResult {
    /// <summary>
    /// Output of the solver
    /// </summary>
    public string ProcessOutput { get; }
    /// <summary>
    /// Status regarding what happened with the process being run
    /// </summary>
    public ProcessStatus Status { get; }
    /// <summary>
    /// The time it took to complete the process
    /// </summary>
    public Times Times { get; }
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

    public SolverResult(string? processOutput, ProcessStatus status, Times times, bool solverCompleted, SATSolution? solution) {
        ProcessOutput = processOutput ?? "";
        Times = times;
        Status = status;
        SolverCompleted = solverCompleted;
        Solution = solution;
    }
}

public struct Times {
    public long Real;
    public long User;
    public long Sys;

    public Times(long real, long user = -1, long sys = -1) {
        Real = real;
        User = user;
        Sys = sys;
    }
}