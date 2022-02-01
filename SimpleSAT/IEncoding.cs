using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

public interface IEncoding<T> {
    List<string> Comments { get; }
    ClauseCollection<T> Hard { get; }
    ClauseCollection<T> Soft { get; }

    void AddHard(params T[] literals);
    void AddSoft(ulong cost, params T[] literals);
    void AddClause(Clause<T> clause);

    string CNFClauseFormat(Clause<T> clause, ulong top);
    string WCNFClauseFormat(Clause<T> clause, ulong top);
}
