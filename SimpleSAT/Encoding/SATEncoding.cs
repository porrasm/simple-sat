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
public class SATEncoding : IEncoding<int> {
    #region fields
    public int LiteralCount { get; private set; }

    public List<string> Comments { get; } = new();

    public ClauseCollection<int> Hard { get; } = new();
    public ClauseCollection<int> Soft { get; } = new();

    public int ClauseCount => Hard.Count + Soft.Count;
    public int HardCount => Hard.Count;
    public int SoftCount => Soft.Count;
    #endregion

    public SATEncoding() { }
    
    public SATEncoding(IEncoding<ProtoLiteral> proto, ProtoLiteralTranslator translator) {
        Hard = ClauseCollection<int>.FromPrevious(proto.Hard);
        Soft = ClauseCollection<int>.FromPrevious(proto.Soft);

        foreach (var hard in proto.Hard.Clauses()) {
            AddClause(translator.TranslateClause(hard));
        }
        foreach (var soft in proto.Soft.Clauses()) {
            AddClause(translator.TranslateClause(soft));
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
        Comments.Add(comment);
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
        AddClause(new Clause<int>(0, literals));
    }

    /// <summary>
    /// Add a comment line before the next hard clause.
    /// </summary>
    public void CommentHard(string comment) {
        Hard.Comment(comment);
    }

    /// <summary>
    /// Add a soft clause. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="literals">Literals of the clause</param>
    public void AddSoft(ulong cost, params int[] literals) {
        AddClause(new Clause<int>(cost, literals));
    }

    /// <summary>
    /// Add a comment line before the next soft clause.
    /// </summary>
    public void CommentSoft(string comment) {
        Soft.Comment(comment);
    }

    /// <summary>
    /// Add a collection of clauses. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="clauses"></param>
    public void AddClauses(IEnumerable<Clause<int>> clauses) {
        foreach (Clause<int> c in clauses) {
            AddClause(c);
        }
    }

    /// <summary>
    /// Add a clause. Each literal under the maximum literal must be present in at least one clause. Otherwise the solver process might fail or provide incorrect results.
    /// </summary>
    /// <param name="clause"></param>
    /// <exception cref="Exception">Clause contained invalid literals</exception>
    public void AddClause(Clause<int> clause) {
        foreach (int literal in clause.Literals) {
            if (literal == 0) {
                throw new Exception("Clause literal cannot be 0");
            }
            if (literal > LiteralCount) {
                LiteralCount = literal;
            }
        }
        if (clause.IsHard) {
            Hard.Add(clause);
        } else {
            Soft.Add(clause);
        }
    }
    #endregion

    #region cnf
    public string CNFClauseFormat(Clause<int> clause, ulong top) => CNF.CNFClauseLine(clause.Literals);
    public string WCNFClauseFormat(Clause<int> clause, ulong top) => CNF.WCNFClauseLine(clause.Literals, clause.IsHard ? top : clause.Cost);

    /// <summary>
    /// Returns the index of the topmost literal + 1
    /// </summary>
    /// <returns></returns>
    public ulong GetIndexAfterTop() {
        ulong top = 0;
        foreach (Clause<int> clause in Soft.Clauses()) {
            top += clause.Cost;
        }
        return top + 1;
    }
    #endregion
}
