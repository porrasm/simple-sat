using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

public class CNFWriter<T> {
    #region fields
    private string outputFile;

    private IEncoding<T> encoding;

    private int literalCount;
    private ulong top;
    #endregion

    public CNFWriter(string outputFile, IEncoding<T> encoding, int literalCount, ulong top = 0) {
        this.outputFile = outputFile;
        this.encoding = encoding;
        this.literalCount = literalCount;
        this.top = top;
    }

    #region cnf
    public void ConvertToCNF() {
        if (encoding.Soft.Count > 0) {
            throw new Exception("Can't convert to CNF if there are soft clauses. Convert to MaxSAT WCNF form instead.");
        }

        File.Delete(outputFile);
        using StreamWriter sw = new StreamWriter(outputFile);

        foreach (string comment in encoding.Comments) {
            sw.WriteLine(CNF.CommentLine(comment));
        }

        sw.WriteLine(CNF.CNFProblemLine(literalCount, encoding.Hard.Count));
        sw.WriteLine(CNF.CommentLine("Hard clauses"));

        foreach (string clause in encoding.Hard.GetSATLines(0, encoding.CNFClauseFormat)) {
            sw.WriteLine(clause);
        }
    }

    public void ConvertToWCNF() {
        File.Delete(outputFile);
        using StreamWriter sw = new(outputFile);

        foreach (string comment in encoding.Comments) {
            sw.WriteLine(CNF.CommentLine(comment));
        }

        sw.WriteLine(CNF.WCNFProblemLine(literalCount, encoding.Hard.Count + encoding.Soft.Count, top));
        sw.WriteLine(CNF.CommentLine("Hard clauses"));

        foreach (string clause in encoding.Hard.GetSATLines(top, encoding.WCNFClauseFormat)) {
            sw.WriteLine(clause);
        }

        sw.WriteLine(CNF.CommentLine("Soft clauses"));

        foreach (string clause in encoding.Soft.GetSATLines(top, encoding.WCNFClauseFormat)) {
            sw.WriteLine(clause);
        }
    }
    #endregion
}
