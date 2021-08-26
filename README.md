# Find Steiner minimum tree (SMT)

## or "my first C# project"



Implementation of multiple algorithms and heuristics to find a SMT:

- Dreyfus Wagner
- Approximation according to Kou, Markowsky and Berman
- Approximation according to Mehlhorn
- 1-Steiner Heuristic

### Background

A minimal Steiner tree can be used to plan traffic networks or pedestrian paths for example. The goal in both scenarios is to connect a set of locations in the most cost-effective way possible.

The minimal Steiner tree problem for undirected graphs is closely related to the minimal spanning tree problem. In both instances, a graph $G = (V, E, \ell)$ is given. Where $V$ stands for the set of nodes and $E$ the set of edges. Each edge is assigned a cost ($\ell$). The minimal spanning tree (MST) is now the set of edges that covers all nodes $V$ and has the smallest sum of costs $\ell$ over all edges $E$.



![explanation](figures\explanation.png)

### Formal Defintion

By solving the Steiner tree problem the goal is to connect all nodes of a subgraph $T$ with minimum cost. The nodes to be connected in $T$ are called terminals $K\subseteq V(T)$. The other nodes $V(T) \setminus K $ are called Steiner nodes.  This report discusses in particular the Steiner problem in a weighted graph, which is also called a network. The problem is defined formatively as follows:


>A connected graph $G = (V, E)$ and a set $K\subseteq V(T)$ >of terminals. A Steiner minimum tree $T$ for $K$ in $G$ >such that $|E(T)|$ = min\{ $|E(T')|$ | $T'$ = Steiner tree >for $K$ in $G$\}

In the case of $|K| = 2$, the Steiner tree is equivalent to the shortest path between the two terminals. In the case of $K = V$, the Steiner tree is equivalent to the minimum spanning tree.

### Found SMT

![example](figures\example.png)



