# simple-sat
A .Net compatible boolean satisfiability encoder

## Lower level encoding

This library provides a simple interface (SimpleSAT.Encoding.SATEncoding class) for creating [Boolean Satisfiability Problem (SAT)](https://en.wikipedia.org/wiki/Boolean_satisfiability_problem) formulas. The library also provides a simple solution parser which can parse the output of a standardized SAT solver using the modern solution output format.

## Higher level encoder

The library also provides a more high level encoder (SimpleSAT.Proto.ProtoEncoder class) which makes the process of creating SAT encodings simpler because the encoder automates literal indexing and auxiliary variable management. The high lever encoder is sligly less efficient because it involves the process of transforming the encoding to the afore mentioned low level encoding first (process is O(N) where N is the total length of the clauses). Slightly more memory is required as well but this is also O(N).
