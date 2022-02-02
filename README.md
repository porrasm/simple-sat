# simple-sat
A .Net compatible boolean satisfiability encoder

[Nuget](https://www.nuget.org/packages/SimpleSAT)

## Lower level encoder

This library provides a simple interface (SimpleSAT.Encoding.SATEncoding class) for creating [Boolean Satisfiability Problem (SAT)](https://en.wikipedia.org/wiki/Boolean_satisfiability_problem) formulas. The library also provides a simple solution parser which can parse the output of a standardized SAT solver using the modern solution output format.

## Higher level encoder

The library also provides a more high level encoder (SimpleSAT.Proto.ProtoEncoder class) which makes the process of creating SAT encodings simpler because the encoder automates literal indexing and auxiliary variable management. The high lever encoder is sligly less efficient because it involves the process of transforming the encoding to the afore mentioned low level encoding first (process is O(N) where N is the total length of the clauses). Slightly more memory is required as well but this is also O(N).

## Debug tools

The package also contains various tools which can help you debug your encoding. For example the CNFWriter class can be used to write a "human readable" CNF file. Comments can be added to describe clauses and each literal can be assigned an arbitrary name which will appear in the readable CNF form instead of a pure integer.

The package also supports solving CNF files using a standardize SAT or MaxSAT solver (e.g. Kissat or MaxHS). You can use the SimpleSAT.SATSolver class to solve CNF files using a path to the SAT solver binary (or you can pass a process instead). The SAT solver process will then be run and it's outut is automatically parsed if the solver ran succesfully.

## Notes

This package is not the most efficient encoder but the running time is dominated by the SAT solver so a slighly inefficient encoder will not cause any noticeable performance impacts. This package is good for experimenting with new encoding schemes and/or debugging encodings as well as for learning purposes for beginners.
