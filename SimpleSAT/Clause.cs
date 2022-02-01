using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

/// <summary>
/// A SAT clause which contains the cost of the clause and the literals of the clause
/// </summary>
public struct Clause<T> {
    #region fields
    public ulong Cost { get; }
    public T[] Literals { get; }
    public bool IsHard => Cost == 0;
    #endregion

    public Clause(ulong cost, T[] literals) {
        Cost = cost;
        Literals = literals;
    }
}