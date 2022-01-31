using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

internal struct SATComment {
    public string Comment;
    public int Index;

    public SATComment(string comment, int index) {
        Comment = comment;
        Index = index;
    }
}
