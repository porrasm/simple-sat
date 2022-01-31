using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT.Proto;

/// <summary>
/// Higher level abstraction of <see cref="Encoding.Clause"/>
/// </summary>
public struct ProtoClause {
    public ulong Cost;
    public bool IsHard => Cost == 0;
    public ProtoLiteral[] Literals;

    public ProtoClause(ulong cost, ProtoLiteral[] literals) {
        this.Cost = cost;
        Literals = literals;
    }
}