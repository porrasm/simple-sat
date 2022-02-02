using SimpleSAT.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Proto;

/// <summary>
/// Trasnlator which can transform a <see cref="ProtoClause"/> into a <see cref="Clause"/>
/// </summary>
public class ProtoLiteralTranslator {
    #region fields
    private Dictionary<ProtoLiteral, int> dict;
    private Dictionary<int, ProtoLiteral> revDict;
    #endregion

    /// <summary>
    /// Generates a translation from a <see cref="ProtoEncoding"/>
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="sortLiteralsByIndex">Sorts the literals based on their index value. Can cause significant performance reduction but makes debugging easier.</param>
    public ProtoLiteralTranslator(ProtoEncoding encoding, bool sortLiteralsByIndex = false) {
        dict = new();
        revDict = new();

        List<ProtoLiteral> literals = new();
        foreach (var variable in encoding.GetVariables()) {
            foreach (ProtoLiteral lit in variable) {
                literals.Add(lit);
            }
        }

        if (sortLiteralsByIndex) {
            literals.Sort((a, b) => {
                int varComp  = a.Variable.CompareTo(b.Variable);
                if (varComp != 0) {
                    return varComp;
                }
                return a.Literal.CompareTo(b.Literal);
            });
        }

        AddLiterals(literals);  
    }

    private void AddLiterals(List<ProtoLiteral> literals) {
        int i = 1;
        foreach (ProtoLiteral lit in literals) {
            Add(lit, i);
            i++;
        }
    }

    /// <summary>
    /// Creates an empty translation
    /// </summary>
    public ProtoLiteralTranslator() {
        dict = new();
        revDict = new();
    }

    /// <summary>
    /// Add a translation for a literal
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Add(ProtoLiteral key, int value) {
        if (value <= 0) {
            throw new Exception("Value must always be greater than or equal to 1");
        }
        if (key.IsNegation) {
            throw new Exception("Literals to add cannot be negations");
        }
        if (key.Literal < 0) {
            throw new Exception("Key literal cannot be negative");
        }
        dict.Add(key, value);
        revDict.Add(value, key);
    }
    public int GetV(ProtoLiteral key) => dict[key];
    public int GetVAssignment(ProtoLiteral key) {
        // todo branchless
        int v = dict[key];
        return key.IsNegation ? -v : v;
    }
    public ProtoLiteral GetK(int value) => revDict[value];

    /// <summary>
    /// Translates a <see cref="ProtoClause"/> into a <see cref="Clause"/>
    /// </summary>
    /// <param name="clause"></param>
    /// <returns></returns>
    public Clause<int> TranslateClause(Clause<ProtoLiteral> clause) {
        return new Clause<int>(clause.Cost, clause.Literals.Select(lit => GetVAssignment(lit)).ToArray());
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach (var pair in dict) {
            sb.AppendLine($"Translate: {pair.Key} = {pair.Value}");
        }
        return sb.ToString();
    }
}
