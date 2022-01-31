using SimpleSAT.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Encoding;

/// <summary>
/// The encoding of some problem instance contains all of its hard and soft clauses. 
/// </summary>
public class SATEncoding {
    #region fields
    public int LiteralCount { get; private set; }

    private List<string> comments = new();

    private ClauseCollection<Clause> hardClauses = new();
    private ClauseCollection<Clause> softClauses = new();

    public int ClauseCount => hardClauses.Count + softClauses.Count;
    public int HardCount => hardClauses.Count;
    public int SoftCount => softClauses.Count;
    #endregion

    public SATEncoding() { }
    
    public SATEncoding(ProtoEncoding proto, ProtoLiteralTranslator translator) {
        hardClauses = ClauseCollection<Clause>.FromPrevious(proto.HardClauses);
        softClauses = ClauseCollection<Clause>.FromPrevious(proto.SoftClauses);

        foreach (var hard in proto.HardClauses.Clauses()) {
            hardClauses.Add(translator.TranslateClause(hard));
        }
        foreach (var soft in proto.SoftClauses.Clauses()) {
            softClauses.Add(translator.TranslateClause(soft));
        }
    }

    #region add
    /// <summary>
    /// Add a comment at the top of the line.
    /// </summary>
    public void CommentGeneral(string comment) {
        if (comment.Contains("\n")) {
            throw new Exception("Comments cannot contain line breaks");
        }
        comments.Add(comment);
    }

    /// <summary>
    /// Add a collection of hard clauses. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="clauses">The set of clauses to add</param>
    public void AddHards(IEnumerable<int[]> clauses) {
        foreach (var clause in clauses) {
            AddHard(clause);
        }
    }

    /// <summary>
    /// Add a hard clause. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="literals">Literals of the clause</param>
    public void AddHard(params int[] literals) {
        AddClause(new Clause(0, literals));
    }

    /// <summary>
    /// Add a comment line before the next hard clause.
    /// </summary>
    public void CommentHard(string comment) {
        hardClauses.Comment(comment);
    }

    /// <summary>
    /// Add a soft clause. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="literals">Literals of the clause</param>
    public void AddSoft(ulong cost, params int[] literals) {
        AddClause(new Clause(cost, literals));
    }

    /// <summary>
    /// Add a comment line before the next soft clause.
    /// </summary>
    public void CommentSoft(string comment) {
        softClauses.Comment(comment);
    }

    /// <summary>
    /// Add a collection of clauses. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="clauses"></param>
    public void AddClauses(IEnumerable<Clause> clauses) {
        foreach (Clause c in clauses) {
            AddClause(c);
        }
    }

    /// <summary>
    /// Add a clause. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="clause"></param>
    /// <exception cref="Exception">Clause contained invalid literals</exception>
    public void AddClause(Clause clause) {
        foreach (int literal in clause.Literals) {
            if (literal == 0) {
                throw new Exception("Clause literal cannot be 0");
            }
            if (literal > LiteralCount) {
                LiteralCount = literal;
            }
        }
        if (clause.IsHard) {
            hardClauses.Add(clause);
        } else {
            softClauses.Add(clause);
        }
    }
    #endregion

    #region cnf
    /// <summary>
    /// Converts the encoding into a SAT solver compatible CNF or WNCF format.
    /// </summary>
    /// <param name="format"></param>
    /// <param name="file"></param>
    public void ConvertToCNF(SATFormat format, string file) {
        if (format == SATFormat.CNF_SAT) {
            ConvertToCNF(file);
        } else {
            ConvertToWCNF(file);
        }
    }

    private void ConvertToCNF(string file) {
        if (softClauses.Count > 0) {
            throw new Exception("Can't convert to CNF if there are soft clauses. Convert to MaxSAT WCNF form instead.");
        }

        File.Delete(file);

        using StreamWriter sw = new StreamWriter(file);

        foreach (string comment in comments) {
            sw.WriteLine(SATLines.CommentLine(comment));
        }

        sw.WriteLine(SATLines.CNFProblemLine(LiteralCount, HardCount));
        sw.WriteLine(SATLines.CommentLine("Hard clauses"));

        foreach (string clause in hardClauses.SATLines(c => SATLines.CNFClauseLine(c.Literals))) {
            sw.WriteLine(clause);
        }
    }

    private void ConvertToWCNF(string file) {
        ulong top = GetTop();

        File.Delete(file);

        using StreamWriter sw = new StreamWriter(file);

        foreach (string comment in comments) {
            sw.WriteLine(SATLines.CommentLine(comment));
        }

        sw.WriteLine(SATLines.WCNFProblemLine(LiteralCount, ClauseCount, top));
        sw.WriteLine(SATLines.CommentLine("Hard clauses"));

        foreach (string clause in hardClauses.SATLines(c => SATLines.WCNFClauseLine(c.Literals, top))) {
            sw.WriteLine(clause);
        }

        sw.WriteLine(SATLines.CommentLine("Soft clauses"));

        foreach (string clause in softClauses.SATLines(c => SATLines.WCNFClauseLine(c.Literals, c.Cost))) {
            sw.WriteLine(clause);
        }
    }

    private ulong GetTop() {
        ulong top = 0;
        foreach (Clause clause in softClauses.Clauses()) {
            top += clause.Cost;
        }
        return top + 1;
    }
    #endregion
}
