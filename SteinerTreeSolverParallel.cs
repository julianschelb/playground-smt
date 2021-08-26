using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SteinerTreeProblem
{
    class SteinerTreeSolverParallel
    {
        private int[,] distanceMatrix;
        private int[,] predecessorMatrix;
        private Dictionary<string, Tuple<List<int[]>, int>> steinertrees;
        private Graph graph;
        private ParallelOptions options;

        // ---------------- Getter and Setter ------------------

        public void saveSteinerTree(List<int> terminals, List<int[]> path, int length)
        {
            // create key for storing steinertree
            terminals.Sort();
            string key = String.Join("-", terminals);
            Tuple<List<int[]>, int> values = Tuple.Create(path, length);

            if (!steinertrees.ContainsKey(key)) {
                steinertrees.Add(key,values);
            }
            else {
                steinertrees[key] = values;
            }
        }

        public Tuple<List<int[]>, int> getSteinerTree(List<int> terminals)
        {
            // create key for finding steinertree
            terminals.Sort();
            string key = String.Join("-", terminals);
            Tuple<List<int[]>, int> values;
            
            if (steinertrees.ContainsKey(key)) {
                values = (Tuple<List<int[]>, int>)this.steinertrees[key];
            }
            else {
                values = Tuple.Create(new List<int[]>(), -1);
            }
             
            return values;
        }

        // ---------------- Constructors ------------------
        
        public SteinerTreeSolverParallel(Graph graph, ParallelOptions options)
        {
            this.graph = graph;
            this.options = options;
            steinertrees = new Dictionary<string, Tuple<List<int[]>, int>>();
        }
        
        // ---------------- Functions ------------------

        public Tuple<List<int[]>, int> solveMinimumSteinerTree() 
        {

            List<int[]> edges = this.graph.getEdges();
            List<int> terminals = this.graph.getTerminals();
            List<int> nodes = this.graph.getNodes();
        
            Console.WriteLine("["+ string.Format("{0:HH:mm:ss}", DateTime.Now) + "] - Starting calculation of minimal Steiner!");
            Console.WriteLine("["+ string.Format("{0:HH:mm:ss}", DateTime.Now) + "] - K = {" + String.Join(",", terminals)+ "}");

            // Create Instance of Dijkstra Algorithm
            graph.calculateDistanceMatrix(this.options);
            distanceMatrix = graph.getDistanceMatrix();
            predecessorMatrix = graph.getPredecessorMatrix();

            // save nodes of shortest paths as steinertree
            this.extractShortestPath();

            Console.WriteLine("["+ string.Format("{0:HH:mm:ss}", DateTime.Now) + "] - Event: Distance Matrix and 2-element Steiner Trees calculated!");

            int k = terminals.Count;

            for (int i = 2; i <= k - 1; i++ ) { 

                Console.WriteLine("["+ string.Format("{0:HH:mm:ss}", DateTime.Now) + "] - Event: Calculating Steiner Trees for |K| = " + (i + 1));

                // determine all subsets of size i
                List<List<int>> subsets = findSubsetsOfSize(terminals, i);
                //List<int> nodesToAdd; // v
                //List<int> nodesInSubset; // w
                //List<int> nodesNotInSubset; // w in case 3

                // Case 1
                foreach(List<int> subset in subsets) 
                {
                    // all v that are not in the susbet X
                    List<int> nodesToAdd = nodes.Except(subset).ToList();

                    // all nodedes to add (v) 
                    foreach(int nodeToAdd in nodesToAdd) {
                    //Parallel.ForEach(nodesToAdd, options, nodeToAdd  =>
                    //{    
                        // all Subsets of the Subset without emty set
                        List<List<int>> subSubsets = findSubsets(subset).ToList();

                        // Go over Sub Subsets (X')
                        foreach(List<int> subSubset in subSubsets) 
                        {
                            // not shure why it does not work
                            List<int> subset2 = subset.ToList();

                            List<int> subSubsetComplement = subset2.Except(subSubset).ToList();

                            // get steiner tree for X' with v
                            List<int> subSubset_and_nodeToAdd = subSubset.ToList();
                            subSubset_and_nodeToAdd.Add(nodeToAdd);
                            Tuple<List<int[]>, int> value = this.getSteinerTree(subSubset_and_nodeToAdd);

                            // get steiner tree for X \ X' with v
                            List<int> subSubsetComplement_and_nodeToAdd = subSubsetComplement.ToList();
                            subSubsetComplement_and_nodeToAdd.Add(nodeToAdd);
                            Tuple<List<int[]>, int> value2 = this.getSteinerTree(subSubsetComplement_and_nodeToAdd);
                            
                            if(value.Item2 >= 0 && value2.Item2 >= 0) {

                                // addd the sizes of both trees
                                int newTreesize = newTreesize = value.Item2 + value2.Item2;

                                // get steiner tree for X with v
                                List<int> subset_and_nodeToAdd = subset2.ToList();
                                subset_and_nodeToAdd.Add(nodeToAdd);
                                Tuple<List<int[]>, int> valueSubsetAndNode = this.getSteinerTree(subset_and_nodeToAdd);

                                // get tree for Subset X
                                Tuple<List<int[]>, int> valueSubset = this.getSteinerTree(subset2);

                                // save new steinertree if overall length is smaller
                                if(valueSubsetAndNode.Item2 > newTreesize || valueSubsetAndNode.Item2 < 0) 
                                {
                                    // combine both trees    
                                    List<int[]> newPath = value.Item1.Concat(value2.Item1).Distinct().ToList();
                                    this.saveSteinerTree(subset_and_nodeToAdd, newPath, newTreesize);
                                }
                            }
                        }
                    }//);
                }

                // Case 2 and Case 3
                
                Parallel.ForEach(subsets, options, subset  =>
                {  
                //foreach(List<int> subset in subsets) 
                //{
                    // all v that are not in the susbet X
                    List<int> nodesToAdd = nodes.Except(subset).ToList();


                    // Case 2

                    // all nodedes to add (v) 
                    foreach(int nodeToAdd in nodesToAdd) 
                    {
                    //Parallel.ForEach(nodesToAdd, options, nodeToAdd  =>
                    //{    
                        // all points w in subset (X) 
                        List<int> nodesInSubset = subset.ToList();

                        foreach(int nodeInSubset in nodesInSubset) 
                        {
                            // not shure why it does not work
                            List<int> subset2 = subset.ToList();

                            // get path from nodeToAdd (v) to nodeInSubset (w)
                            List<int> nodeToAdd_and_nodeInSubset  = new List<int>() {nodeToAdd, nodeInSubset} ;
                            Tuple<List<int[]>, int> value = this.getSteinerTree(nodeToAdd_and_nodeInSubset);

                            // get steiner tree for the subset X
                            Tuple<List<int[]>, int> valueSubset = this.getSteinerTree(subset2);

                            if(valueSubset.Item2 >= 0 && value.Item2 >= 0) 
                            {
                                // Combined  tree length
                                int newTreesize = value.Item2 + valueSubset.Item2;

                                // get steiner tree for X with v
                                List<int> subset_and_nodeToAdd = subset2.ToList();
                                subset_and_nodeToAdd.Add(nodeToAdd);
                                Tuple<List<int[]>, int> valueSubsetAndNode = this.getSteinerTree(subset_and_nodeToAdd);

                                // save new steinertree if overall length is smaller
                                if(valueSubsetAndNode.Item2 > newTreesize || valueSubsetAndNode.Item2 < 0) 
                                {
                                    // combine both trees    
                                    List<int[]> newPath = value.Item1.Concat(valueSubset.Item1).Distinct().ToList();
                                    this.saveSteinerTree(subset_and_nodeToAdd, newPath, newTreesize);
                                }
                            }
                        }
                        

                        // Case 3

                        // all points w not in subset (X) 
                        List<int> nodesNotInSubset = nodes.Except(subset).ToList();

                        foreach(int nodeNotInSubset in nodesNotInSubset) 
                        {
                            // not shure why it does not work
                            List<int> subset2 = subset.ToList();

                            // get path from nodeToAdd (v) to nodeInSubset (w)
                            List<int> nodeToAdd_and_nodeNotInSubset  = new List<int>() {nodeToAdd, nodeNotInSubset} ;
                            Tuple<List<int[]>, int> value = this.getSteinerTree(nodeToAdd_and_nodeNotInSubset);

                            // get steiner tree for X with w
                            List<int> subset_and_nodeNotInSubset = subset2.ToList();
                            subset_and_nodeNotInSubset.Add(nodeNotInSubset);
                            Tuple<List<int[]>, int> valueSubsetAndNodeNotIn = this.getSteinerTree(subset_and_nodeNotInSubset);

                            if(valueSubsetAndNodeNotIn.Item2 >= 0 && value.Item2 >= 0) 
                            {
                                int newTreesize = value.Item2 + valueSubsetAndNodeNotIn.Item2;

                                // get steiner tree for X with v
                                List<int> subset_and_nodeToAdd = subset2.ToList();
                                subset_and_nodeToAdd.Add(nodeToAdd);
                                Tuple<List<int[]>, int> valueSubsetAndNode = this.getSteinerTree(subset_and_nodeToAdd);

                                // save new steinertree if overall length is smaller
                                if(valueSubsetAndNode.Item2 > newTreesize || valueSubsetAndNode.Item2 < 0) 
                                {
                                    // combine both trees    
                                    List<int[]> newPath = value.Item1.Concat(valueSubsetAndNodeNotIn.Item1).Distinct().ToList();
                                    this.saveSteinerTree(subset_and_nodeToAdd, newPath, newTreesize);
                                }
                            }
                        }
                    }//);
                });
            }



            //Tuple<List<int[]>, int> valueTerminals = ;
            //Console.WriteLine("Found minimal Tree = " + String.Join(",", valueTerminals.Item1));
            Console.WriteLine("["+ string.Format("{0:HH:mm:ss}", DateTime.Now) + "] - Result: Found Tree with minimal Length = " + this.getSteinerTree(terminals).Item2);
            Console.WriteLine("------------------------------------------------------------");

            return this.getSteinerTree(terminals);         
        }

        public List<List<int>> findSubsets(List<int> source)
        {
            return findSubsetsOfSize(source, 0);
        }

        public List<List<int>> findSubsetsOfSize(List<int> source, int size) 
        {
            
            List<List<int>> subsets = new List<List<int>>();

            for (int i = 0; i < Math.Pow(2, source.Count); i++)
            {
                List<int> combination = new List<int>();
                for (int j = 0; j < source.Count; j++)
                {
                    if ((i & (1 << (source.Count - j - 1))) != 0)
                    {
                        combination.Add(source[j]);
                    }
                }

                if(size > 0) 
                {
                    if (combination.Count == size) subsets.Add(combination);
                }
                else 
                {
                    if(combination.Count > 0 & combination.Count < source.Count) subsets.Add(combination);
                } 
            }
            return subsets;
        }

        public void extractShortestPath() 
        {     
            // init variables
            List<int> currentTerminals;
            List<int[]> path;
            int n = this.graph.getNodes().Count();

            // Shortest Path from every root node ...
            for(int i = 1; i <= n; i++) 
            {
                // to every other point
                for(int j = 1; j <= n; j++) 
                {
                    // init list for all nodes in the steiner tree            
                    path = new List<int[]>();

                    // set destination point as startin gpoint for backtracking
                    int predecessor = j;

                    // backtrack to root point i
                    while(i != predecessor) 
                    {
                        path.Add(new int[] {predecessor, this.predecessorMatrix[i,predecessor], this.distanceMatrix[predecessor,this.predecessorMatrix[i,predecessor]]});
                        predecessor = this.predecessorMatrix[i,predecessor];
                    }
                    
                    // add root point to path
                    // path.Add(predecessor);

                    // write terminals in a list
                    currentTerminals = new List<int>() {i, j};

                    //Write into Hastree
                    this.saveSteinerTree(currentTerminals, path, this.distanceMatrix[i,j]);
                }
            }

        }

    }
}
