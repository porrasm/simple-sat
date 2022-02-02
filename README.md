# simple-sat
A .Net compatible boolean satisfiability encoder

[Nuget](https://www.nuget.org/packages/SimpleSAT)

## Examples

Comprehensive examples can be found within [this repository](https://github.com/porrasm/correlation-clustering-maxsat) which contains different encodings for the weighted correlation clustering problem.

## Lower level encoder

This library provides a simple interface (SimpleSAT.Encoding.SATEncoding class) for creating [Boolean Satisfiability Problem (SAT)](https://en.wikipedia.org/wiki/Boolean_satisfiability_problem) formulas. The library also provides a simple solution parser which can parse the output of a standardized SAT solver using the modern solution output format.

## Higher level encoder

The library also provides a more high level encoder (SimpleSAT.Proto.ProtoEncoder class) which makes the process of creating SAT encodings simpler because the encoder automates literal indexing and auxiliary variable management. The high lever encoder is sligly less efficient because it involves the process of transforming the encoding to the afore mentioned low level encoding first (process is O(N) where N is the total length of the clauses). Slightly more memory is required as well but this is also O(N).

## Notes

This package is not the most efficient encoder but the running time is dominated by the SAT solver so a slighly inefficient encoder will not cause any noticeable performance impacts. This package is good for experimenting with new encoding schemes and/or debugging encodings as well as for learning purposes for beginners.


## Debug tools

The package also contains various tools which can help you debug your encoding. For example the CNFWriter class can be used to write a "human readable" CNF file. Comments can be added to describe clauses and each literal can be assigned an arbitrary name which will appear in the readable CNF form instead of a pure integer.

The package also supports solving CNF files using a standardize SAT or MaxSAT solver (e.g. Kissat or MaxHS). You can use the SimpleSAT.SATSolver class to solve CNF files using a path to the SAT solver binary (or you can pass a process instead). The SAT solver process will then be run and it's outut is automatically parsed if the solver ran succesfully.

### Demonstration

Below are some demonstration of what the readable CNF files look like. A [transitive encoding](https://github.com/porrasm/correlation-clustering-maxsat/blob/main/correlation-clustering-encoder/Encoder/Implementations/TransitiveEncoding.cs) for the correlation clustering problem was used here with an input matrix which had 4 different data points.

#### Regular WCNF file
```
p wcnf 6 18 6001
c Hard clauses
6001 -1 -2 3 0
6001 -1 -3 2 0
6001 -3 -2 1 0
6001 -1 -4 5 0
6001 -1 -5 4 0
6001 -5 -4 1 0
6001 -3 -6 5 0
6001 -3 -5 6 0
6001 -5 -6 3 0
6001 -2 -6 4 0
6001 -2 -4 6 0
6001 -4 -6 2 0
c Soft clauses
1000 1 0
1000 3 0
1000 -5 0
1000 -2 0
1000 4 0
1000 6 0

```

#### Readable WCNF file
```
p wcnf -1 18 0
c Hard clauses
H: -SameCluster[0, 1] -SameCluster[1, 2] SameCluster[0, 2]
H: -SameCluster[0, 1] -SameCluster[0, 2] SameCluster[1, 2]
H: -SameCluster[0, 2] -SameCluster[1, 2] SameCluster[0, 1]
H: -SameCluster[0, 1] -SameCluster[1, 3] SameCluster[0, 3]
H: -SameCluster[0, 1] -SameCluster[0, 3] SameCluster[1, 3]
H: -SameCluster[0, 3] -SameCluster[1, 3] SameCluster[0, 1]
H: -SameCluster[0, 2] -SameCluster[2, 3] SameCluster[0, 3]
H: -SameCluster[0, 2] -SameCluster[0, 3] SameCluster[2, 3]
H: -SameCluster[0, 3] -SameCluster[2, 3] SameCluster[0, 2]
H: -SameCluster[1, 2] -SameCluster[2, 3] SameCluster[1, 3]
H: -SameCluster[1, 2] -SameCluster[1, 3] SameCluster[2, 3]
H: -SameCluster[1, 3] -SameCluster[2, 3] SameCluster[1, 2]
c Soft clauses
S 1000: SameCluster[0, 1]
S 1000: SameCluster[0, 2]
S 1000: -SameCluster[0, 3]
S 1000: -SameCluster[1, 2]
S 1000: SameCluster[1, 3]
S 1000: SameCluster[2, 3]
```

#### Readable assignments
This is one plausible optimal solution to the problem above. It has cost 2000 since points [0, 3] and [1, 2] should not be in the same cluster (both worth 1000 penalty) but in this solution they are.
```
+SameCluster[0, 1]
+SameCluster[1, 2]
+SameCluster[0, 2]
+SameCluster[1, 3]
+SameCluster[0, 3]
+SameCluster[2, 3]
```