using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Proto;

/// <summary>
/// A 1-dimensional representation of an auxiliary SAT variable
/// </summary>
public class ProtoVariable {
    #region fields
    private ProtoEncoding encoding;
    private int offset;
    public byte variable { get; }
    #endregion

    internal ProtoVariable(ProtoEncoding encoding, byte variable, int offset) {
        this.encoding = encoding;
        this.variable = variable;
        this.offset = offset;
    }

    public ProtoVariable(ProtoEncoding encoding, byte variable) {
        this.encoding = encoding;
        this.variable = variable;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[int dim0] {
        get {
            ProtoLiteral lit = new ProtoLiteral(variable, dim0 + offset);
            encoding.Register(lit);
            return lit;
        }
    }
}

/// <summary>
/// A 2-dimensional representation of an auxiliary SAT variable
/// </summary>
public class ProtoVariable2D {
    #region fields
    private ProtoEncoding encoding;
    public byte variable { get; }
    private int dim1Size;
    private bool symmetric;
    public string? Name { get; set; }
    #endregion

    /// <summary>
    /// Create a new 2D aux variable.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="dim1Size">The maximum size of the last dimension</param>
    /// <param name="symmetric">If set to true, the variable will be interpreted as symmetric so the index [0, 1] gives the same result as [1, 0].</param>
    public ProtoVariable2D(ProtoEncoding encoding, int dim1Size, bool symmetric = false, string? name = null) {
        this.encoding = encoding;
        this.variable = encoding.CreateNewVariable();
        this.dim1Size = dim1Size;
        this.symmetric = symmetric;
        Name = name;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[int dim0, int dim1] {
        get {
            if (symmetric) {
                FixIndices(ref dim0, ref dim1);
            }
            ProtoLiteral lit = new ProtoLiteral(variable, (dim0 * dim1Size) + dim1).Named(Name, dim0, dim1);
            encoding.Register(lit);
            return lit;
        }
    }

    public ProtoLiteral Named(string name, int dim0, int dim1) => this[dim0, dim1].Named(name, dim0, dim1);

    private void FixIndices(ref int dim0, ref int dim1) {
        int dim0s = dim0;
        dim0 = dim0 < dim1 ? dim0 : dim1;
        dim1 = dim1 > dim0s ? dim1 : dim0s;
    }

    /// <summary>
    /// Generates a 1D variable within the scope of this 2D variable. Useful when you need a collection of distinct 1D variables.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public ProtoVariable Generate1DVariable(int index) {
        int offset = index * dim1Size;
        return new ProtoVariable(encoding, variable, offset);
    }

    /// <summary>
    /// Destructures the indices of this variable
    /// </summary>
    /// <param name="literalIndex"></param>
    /// <param name="dim0"></param>
    /// <param name="dim1"></param>
    public void GetParameters(int literalIndex, out int dim0, out int dim1) {
        dim0 = literalIndex / dim1Size;
        dim1 = literalIndex % dim1Size;
    }
}

/// <summary>
/// A 3-dimensional representation of an auxiliary SAT variable
/// </summary>
public class ProtoVariable3D {
    #region fields
    private ProtoEncoding encoding;
    public byte variable { get; }
    private int dim1Size, dim2Size;

    public string? Name { get; set; }
    #endregion

    /// <summary>
    /// Create a new 2D aux variable.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="dim1Size">The max size of the 2nd dimension</param>
    /// <param name="dim2Size">The max size of the 3rd dimension</param>
    public ProtoVariable3D(ProtoEncoding encoding, int dim1Size, int dim2Size, string? name = null) {
        this.encoding = encoding;
        this.variable = encoding.CreateNewVariable();
        this.dim1Size = dim1Size;
        this.dim2Size = dim2Size;
        Name = name;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[int dim0, int dim1, int dim2] {
        get {
            ProtoLiteral lit = new ProtoLiteral(variable, (dim0 * dim1Size * dim2Size) + (dim1 * dim2Size) + dim2).Named(Name, dim0, dim1, dim2);
            encoding.Register(lit);
            return lit;
        }
    }

    /// <summary>
    /// Destructures the indices of this variable
    /// </summary>
    /// <param name="literalIndex"></param>
    /// <param name="dim0"></param>
    /// <param name="dim1"></param>
    /// <param name="dim2"></param>
    public void GetParameters(int literalIndex, out int dim0, out int dim1, out int dim2) {
        dim0 = literalIndex / (dim1Size * dim2Size);
        dim1 = literalIndex / dim2Size % dim1Size;
        dim2 = literalIndex % dim2Size;
    }
}

public interface IProtoVariableSet {
    public ProtoLiteral this[params int[] indices] { get; }
    public ProtoLiteral Named(string name, params int[] indices);
    public string? Name { get; }
    public byte Variable { get; }
    public int[] GetParameters(int literalIndex);
}

/// <summary>
/// An n-dimensional representation of auxiliary SAT variables
/// </summary>
public class ProtoVariableSet : IProtoVariableSet {
    #region fields
    private ProtoEncoding encoding;
    public byte Variable { get; }
    public string? Name { get; set; }

    private struct IntArray {
        public int[] array;
        public int hash;

        public IntArray(int[] array) {
            this.array = array;
            hash = 0;
            for (int i = 0; i < array.Length; i++) {
                hash = hash * 31 + array[i];
            }
        }

        public override int GetHashCode() => hash;
        public override bool Equals(object? obj) {
            if (obj is IntArray other) {
                if (other.array.Length != array.Length) {
                    return false;
                }
                for (int i = 0; i < array.Length; i++) {
                    if (other.array[i] != array[i]) {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }

    private int nextIndex = 0;

    private Dictionary<IntArray, int> indexCache = new Dictionary<IntArray, int>();
    private Dictionary<int, int[]> reverseIndexCache = new Dictionary<int, int[]>();
    #endregion

    /// <summary>
    /// Create a new auxiliary variable set.
    /// </summary>
    /// <param name="encoding"></param>
    public ProtoVariableSet(ProtoEncoding encoding, string? name = null) {
        this.encoding = encoding;
        this.Variable = encoding.CreateNewVariable();
        Name = name;
    }

    /// <summary>
    /// Returns the literal and registers it to the encoding
    /// </summary>
    /// <returns></returns>
    public ProtoLiteral this[params int[] indices] {
        get {
            if (!indexCache.TryGetValue(new IntArray(indices), out int index)) {
                index = nextIndex++;
                indexCache.Add(new IntArray(indices), index);
                reverseIndexCache.Add(index, indices);
            }

            ProtoLiteral lit = new ProtoLiteral(Variable, index).Named(Name, indices);
            encoding.Register(lit);
            return lit;
        }
    }

    /// <summary>
    /// Returns the literal with a name and registers it to the encoding
    /// </summary>
    public ProtoLiteral Named(string name, params int[] indices) => this[indices].Named(name, indices);

    /// <summary>
    /// Destructures the indices of this variable
    /// </summary>
    /// <param name="literalIndex"></param>
    public int[] GetParameters(int literalIndex) => reverseIndexCache[literalIndex];

    public IProtoVariableSet GetPrefixedSubset(params int[] prefix) => new SubProtoVariableSet(this, prefix);
}

public class SubProtoVariableSet : IProtoVariableSet {
    #region fields
    private int[] prefix;
    private ProtoVariableSet variableSet;
    public string? Name => variableSet.Name;

    public byte Variable => variableSet.Variable;
    #endregion

    public SubProtoVariableSet(ProtoVariableSet variableSet, params int[] prefix) {
        this.variableSet = variableSet;
        this.prefix = prefix;
    }

    public ProtoLiteral this[params int[] indices] => variableSet[prefix.Concat(indices).ToArray()];
    public ProtoLiteral Named(string name, params int[] indices) => this[indices].Named(name, indices);
    public int[] GetParameters(int literalIndex) => variableSet.GetParameters(literalIndex).Skip(prefix.Length).ToArray();
}