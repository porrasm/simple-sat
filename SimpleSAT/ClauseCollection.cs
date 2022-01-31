using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSAT;

/// <summary>
/// A collection which contains clauses paired with comments. Comments are useful for debugging encodings.
/// </summary>
/// <typeparam name="T">The clause type to use</typeparam>
public class ClauseCollection<T> {
    #region fields
    private List<T> clauses = new();
    private List<SATComment> comments = new();

    public int Count => clauses.Count;
    #endregion

    public void Add(T item) => clauses.Add(item);

    public void Comment(string comment) {
        if (comment.Contains("\n")) {
            throw new Exception("Comments cannot contain line breaks");
        }
        comments.Add(new SATComment(comment, clauses.Count));
    }

    public IEnumerable<T> Clauses() {
        foreach (T clause in clauses) {
            yield return clause;
        }
    }

    internal IEnumerable<string> SATLines(Func<T, string> clauseFormat) {
        int commentIndex = 0;
        SATComment comment = GetComment(commentIndex);

        for (int i = 0; i < clauses.Count; i++) {
            while (comment.Index == i) {
                yield return comment.Comment;
                comment = GetComment(++commentIndex);
            }
            yield return clauseFormat(clauses[i]);
        }
    }

    private SATComment GetComment(int index) {
        return index < comments.Count ? comments[index] : new SATComment("", -1);
    }

    internal static ClauseCollection<T> FromPrevious<Y>(ClauseCollection<Y> prev) {
        ClauseCollection<T> collection = new ClauseCollection<T>();
        collection.comments = prev.comments;
        return collection;
    }
}
