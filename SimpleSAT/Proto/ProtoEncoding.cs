using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Proto;

/// <summary>
/// A more high level abstraction of <see cref="Encoding.SATEncoding"/>. Contains useful features such as automatic literal indexing and easier variable management but is slightly less efficient.
/// </summary>
public class ProtoEncoding : IEncoding<ProtoLiteral> {
    #region fields
    private List<HashSet<ProtoLiteral>> variables = new();
    private List<string?> names = new();

    public List<string> Comments { get; } = new();
    public ClauseCollection<ProtoLiteral> Hard { get; private set; } = new();
    public ClauseCollection<ProtoLiteral> Soft { get; private set; } = new();
    #endregion

    /// <summary>
    /// Iterates through the added variables
    /// </summary>
    /// <returns></returns>
    public IEnumerable<HashSet<ProtoLiteral>> GetVariables() {
        foreach (var variableSEt in variables) { 
            yield return variableSEt;
        }
    }

    /// <summary>
    /// Creates a new literal variable index.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public byte CreateNewVariable(string? name = null) {
        if (variables.Count >= 127) {
            throw new Exception("Maximum variable count reached");
        }
        byte newIndex = (byte)variables.Count;
        variables.Add(new());
        names.Add(name);
        return newIndex;
    }

    /// <summary>
    /// Returns a new proto literal.
    /// </summary>
    /// <param name="variableIndex"></param>
    /// <param name="literalIndex"></param>
    /// <returns></returns>
    public ProtoLiteral GetLiteral(byte variableIndex, int literalIndex) {
        ProtoLiteral lit = new ProtoLiteral(variableIndex, literalIndex);
        variables[variableIndex].Add(lit);
        return lit;
    }

    /// <summary>
    /// Registers a proto literal for future low level indexing
    /// </summary>
    /// <param name="lit"></param>
    /// <returns></returns>
    public bool Register(ProtoLiteral lit) {
        if (lit.IsNegation) {
            throw new Exception("Cannot register a negation of a literal");
        }
        if (lit.Literal < 0) {
            throw new Exception("Literal index cannot be negative");
        }
        return variables[lit.Variable].Add(lit);
    }

    /// <summary>
    /// Add a collection hard clauses.
    /// </summary>
    /// <param name="clauses"></param>
    public void AddHards(IEnumerable<ProtoLiteral[]> clauses) {
        foreach (var clause in clauses) {
            AddHard(clause);
        }
    }

    /// <summary>
    /// Add a hard clause.
    /// </summary>
    /// <param name="literals"></param>
    public void AddHard(params ProtoLiteral[] literals) {
        Hard.Add(new(0, literals));
    }

    /// <summary>
    /// Add a soft clause.
    /// </summary>
    /// <param name="cost"></param>
    /// <param name="literals"></param>
    public void AddSoft(ulong cost, params ProtoLiteral[] literals) {
        Soft.Add(new(cost, literals));
    }

    /// <summary>
    /// Add a comment before the next hard clause
    /// </summary>
    /// <param name="c"></param>
    public void CommentHard(string c) {
        Hard.Comment(c);
    }

    /// <summary>
    /// Add a comment before the next soft clause
    /// </summary>
    /// <param name="c"></param>
    public void CommentSoft(string c) {
        Soft.Comment(c);
    }

    public void AddClause(Clause<ProtoLiteral> clause) {
        if (clause.IsHard) {
            Hard.Add(clause);
        } else {
            Soft.Add(clause);
        }
    }

    public string CNFClauseFormat(Clause<ProtoLiteral> clause, ulong top) => $"{string.Join(" ", clause.Literals.Select(l => l.GetDisplayString()))}";
    public string WCNFClauseFormat(Clause<ProtoLiteral> clause, ulong top) => $"{(clause.IsHard ? "HARD" : $"SOFT {clause.Cost}")}: {string.Join(" ", clause.Literals.Select(l => l.GetDisplayString()))}";
}
