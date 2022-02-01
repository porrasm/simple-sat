using SimpleSAT.Encoding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

/// <summary>
/// This class can generate CNF or WCNF format compatible lines
/// </summary>
public static class CNF {
    public static string CNFClauseLine(int[] clause) {
        return $"{string.Join(" ", clause)} 0";
    }
    public static string WCNFClauseLine(int[] clause, ulong cost) {
        return $"{cost} {string.Join(" ", clause)} 0";
    }

    public static string CNFProblemLine(int literalCount, int clauseCount) {
        return $"p cnf {literalCount} {clauseCount}";
    }
    public static string WCNFProblemLine(int literalCount, int clauseCount, ulong top) {
        return $"p wcnf {literalCount} {clauseCount} {top}";
    }

    public static string CommentLine(string comment) {
        return $"c {comment}";
    }


}