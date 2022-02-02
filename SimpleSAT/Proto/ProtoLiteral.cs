using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Proto;

/// <summary>
/// Higher level abstraction of an integer literal. The only difference to a low level integer literal is that the <see cref="ProtoLiteral"/> does not have a known index yet.
/// </summary>
public struct ProtoLiteral {
    #region fields
    private const byte NEGATION_MASK = 128;
    private const byte VARIABLE_MASK = 127;

    private byte data;
    public readonly int Literal;

    public bool IsNegation => (data & NEGATION_MASK) != 0;
    public int Variable => data & VARIABLE_MASK;
    public string? Name { get; set; }
    #endregion

    /// <summary>
    /// Returns a negation of this literal
    /// </summary>
    public ProtoLiteral Neg {
        get {
            ProtoLiteral neg = this;
            neg.data = (byte)(data | NEGATION_MASK);
            return neg;
        }
    }

    /// <summary>
    /// Returns a nonnegative assignment of this literal
    /// </summary>
    public ProtoLiteral Pos {
        get {
            ProtoLiteral neg = this;
            neg.data = (byte)(data & VARIABLE_MASK);
            return neg;
        }
    }

    /// <summary>
    /// Creates a new literal
    /// </summary>
    /// <param name="variable">The variable this literal belongs to</param>
    /// <param name="literalIndex">The index of this literal within the variable</param>
    /// <exception cref="Exception"></exception>
    public ProtoLiteral(int variable, int literalIndex) {
        if (variable > 127) {
            throw new Exception("Current model supports up to 127 auxiliary variables in an encoding");
        }
        this.data = (byte)variable;
        this.Literal = literalIndex;
        Name = null;
    }

    public ProtoLiteral Named(string? name, params object[] indices) {
        if (name == null || name.Length == 0) {
            return this;
        }
        if (indices.Length == 0) {
            Name = name;
            return this;
        }
        Name = $"{name}[{string.Join(", ", indices.Select(s => s.ToString()))}]";
        return this;
    }

    public override bool Equals(object? obj) {
        return obj is ProtoLiteral literal &&
               Literal == literal.Literal &&
               Variable == literal.Variable;
    }

    public override int GetHashCode() {
        return HashCode.Combine(Literal, Variable);
    }

    public string GetDisplayString(bool positiveSign = false) {
        return Name == null ? ToString() : $"{(IsNegation ? "-" : (positiveSign ? "+" : ""))}{Name}";
    }

    public override string ToString() => $"({Variable}, {Literal}, neg=${IsNegation})";
}