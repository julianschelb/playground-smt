using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace SteinerTreeProblem
{
    class SteinerTreeMehlhornApprox
    {
        private int[,] distanceMatrix;
        private int[,] predecessorMatrix;
        private Dictionary <string,Tuple<List<int[]>, int>> paths;
        private Graph graph;
        private ParallelOptions options;
        private int length;

        // ---------------- Getter and Setter ------------------

        public int getLength()
        {
            return this.length;
        }

        public void savePath(List<int> terminals, List<int[]> path, int length)
        {
            // create key for storing steinertree
            terminals.Sort();
            string key = String.Join("-", terminals);
            Tuple<List<int[]>, int> values = Tuple.Create(path, length);

            if (!paths.ContainsKey(key)) paths.Add(key,values);
            else paths[key] = values;
        }

        public Tuple<List<int[]>, int> getPath(List<int> terminals)
        {
            // create key for finding steinertree
            terminals.Sort();
            string key = String.Join("-", terminals);
            Tuple<List<int[]>, int> values;
            
            if (paths.ContainsKey(key)) values = (Tuple<List<int[]>, int>)this.paths[key];
            else values = Tuple.Create(new List<int[]>(), int.MaxValue);
            
            return values;
        }

        // ---------------- Constructors ------------------
        
        public SteinerTreeMehlhornApprox(Graph graph, ParallelOptions options)
        {
            this.graph = graph;
            this.options = options;
            paths = new Dictionary <string,Tuple<List<int[]>, int>>();
        }

        // ---------------- Functions ------------------
        
         public List<int[]> approxMinimumSteinerTree(List<int> terminals) 
        {
            this.graph.setTerminals(terminals);
            return(approxMinimumSteinerTree());
        }

        public List<int[]> approxMinimumSteinerTree() 
        {
            List<int[]> edges = graph.getEdges();
            List<int> terminals = graph.getTerminals();
            List<int> nodes = graph.getNodes();
            length = 0;

            // Create Instance of Dijkstra Algorithm
            graph.calculateDistanceMatrixTerminals(this.options);
            distanceMatrix = graph.getDistanceMatrix();
            predecessorMatrix = graph.getPredecessorMatrix();

            // Create Distance Network -------------------------------------

            // save nodes of shortest paths as steinertree
            this.extractShortestPath();

            // create a new graph as full distance Network
            Graph distanceNetwork = new Graph(distanceMatrix, graph.getTerminals());

            // create Instance of Prims Mimumim Spannign Tree Algorithm
            List<int[]> mstList = distanceNetwork.solveMinimumSpanningTree();

            // Replace Edges with the actual Path -------------------------
            List<int[]> steinerTree = new List<int[]>();

            foreach(int[] edge in mstList) 
            { 
                terminals = new List<int>() {edge[0], edge[1]};
                steinerTree = steinerTree.Concat(getPath(terminals).Item1).Distinct().ToList();                
                length = length + edge[2];
            }

            return steinerTree;
        }

        public void extractShortestPath() 
        {     
            // init variables
            List<int> currentTerminals;
            List<int[]> path;
            int n = this.graph.getNodes().Count();

            // Shortest Path from every root node ...
            //for(int i = 1; i <= n; i++)
            foreach(int i in graph.getTerminals()) 
            {
                // to every other point
                foreach(int j in graph.getTerminals()) 
                {

                    // init list for all nodes in the steiner tree            
                    path = new List<int[]>();

                    // set destination point as startin gpoint for backtracking
                    int predecessor = j;

                    // backtrack to root point i
                    while(i != predecessor) 
                    {
                        path.Add(new int[] {predecessor, this.predecessorMatrix[i,predecessor], graph.getWeight(predecessor,this.predecessorMatrix[i,predecessor])});                   
                        predecessor = this.predecessorMatrix[i,predecessor]; 

                    }

                    // write terminals in a list
                    currentTerminals = new List<int>() {i, j};

                    // save path
                    savePath(currentTerminals, path, this.distanceMatrix[i,j]);

                    //break;
                }
               //break; 
            }
        }
    }
}
