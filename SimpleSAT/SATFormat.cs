using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

/// <summary>
/// The two supported CNF output formats of this encoder
/// </summary>
public enum SATFormat {
    CNF_SAT,
    WCNF_MAXSAT
}
