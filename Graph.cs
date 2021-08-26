using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteinerTreeProblem
{
    class Graph
    {
        private List<int> nodes;
        private List<int[]> edges;
        private List<int> terminals;
        private int length;
        private Dictionary<int, List<int[]>> adjacentEdges;
        private int[,] distanceMatrix;
        private int[,] predecessorMatrix;

        // ---------------- Getter and Setter ------------------

        public List<int> getTerminals()
        {
            return terminals;
        }

        public void setTerminals(List<int> value)
        {
            terminals = value;
        }

        public List<int[]> getEdges()
        {
            return edges;
        }

        public void setEdges(List<int[]> value)
        {
            edges = value;
        }

        public List<int> getNodes()
        {
            return nodes;
        }

        public void setNodes(List<int> value)
        {
            nodes = value;
        }

        public int[,] getDistanceMatrix()
        {
            return this.distanceMatrix;
        }

        public int[,] getPredecessorMatrix()
        {
            return this.predecessorMatrix;
        }

        public int getLength()
        {
            return this.length;
        }
        
        // ---------------- Constructors ------------------

        public Graph(String inputFilePath)
        {
            // init list for edges and terminals
            this.edges = new List<int[]>();
            this.terminals = new List<int>();
            this.nodes =  new List<int>();
            this.adjacentEdges = new Dictionary<int, List<int[]>>();
            this.length = 0;

            // Import Graph Data
            string[] lines = System.IO.File.ReadAllLines(inputFilePath);

            foreach (string line in lines)
            {
                // split line using space
                string[] values = line.Split(' ');

                if(values[0] == "E") addEdge(new int[] {int.Parse(values[1]), int.Parse(values[2]), int.Parse(values[3])}); // import edges
                else if (values[0] == "T") getTerminals().Add(int.Parse(values[1])); // import terminals
                //else if (values[0] != "END" & values[0] != "EOF")  Console.WriteLine("\t" + line); // print meta informations but EOF
            }

        }

        public Graph(int[,] distanceMatrix, List<int> terminals)
        {
            // init list for edges and terminals
            this.edges = new List<int[]>();
            this.nodes =  new List<int>();
            this.adjacentEdges = new Dictionary<int, List<int[]>>();
            this.length = 0;

             // set init variables
            this.setTerminals(terminals);

            foreach(int i in terminals) 
            {
                foreach(int j in terminals) 
                {
                    addEdge(new int[] {i, j, distanceMatrix[i,j]});

                    //Console.WriteLine(i + " " + j + " " +  distanceMatrix[i,j]);
                }
            }
        }

        public Graph(List<int[]> edgeList, List<int> terminalList)
        {
            createGraphFromEdgeList(edgeList, terminalList); 
        }
        

        // ---------------- Functions ------------------

        private void createGraphFromEdgeList(List<int[]> edgeList, List<int> terminalList) 
        {
            // init list for edges and terminals
            this.nodes = new List<int>();
            this.edges = new List<int[]>();
            this.adjacentEdges = new Dictionary<int, List<int[]>>();
            this.length = 0;

            // set init variables
            //this.setEdges(edgeList);
            this.setTerminals(terminalList);

            // import edges 
            foreach (int[] edge in edgeList.ToList()) addEdge(edge); 
        }

        private void addEdge(int[] node) 
        {       

            if(node[0] != node[1]) {   
                // increase length
                this.length = length + node[2];

                // add to list of edges from node - to node - weight
                this.getEdges().Add(new int[] { node[0], node[1], node[2]});

                // add from node to node list of not exists already
                if(!getNodes().Contains(node[0])) getNodes().Add(node[0]);

                // add to node to node list of not exists already
                if(!getNodes().Contains(node[1])) getNodes().Add(node[1]);

                // Populate list of adjacent nodes per node 
                if(!adjacentEdges.ContainsKey(node[0])) adjacentEdges.Add(node[0], new List<int[]>(){});
                if(!adjacentEdges.ContainsKey(node[1])) adjacentEdges.Add(node[1], new List<int[]>(){});

                // New adjacent Node for from Node
                List<int[]> newAdjacentEdges = adjacentEdges[node[0]];
                int[] connection = new int[]  { node[0], node[1], node[2]};

                if(!newAdjacentEdges.Contains(connection)) 
                {
                    newAdjacentEdges.Add(connection);
                    adjacentEdges[node[0]] = newAdjacentEdges;
                }

                // New adjacent Nodoe for to Node
                List<int[]> newAdjacentEdges2 = adjacentEdges[node[1]];
                int[] connection2 = new int[]  { node[1], node[0], node[2]};

                if(!newAdjacentEdges2.Contains(connection2)) 
                {
                    newAdjacentEdges2.Add(connection2);
                    adjacentEdges[node[1]] = newAdjacentEdges2;
                }
            }
        }

        public List<int[]> getAdjacentNodes(int node) 
        {
            return  adjacentEdges[node];
        }

        public int getWeight(int node, int node2) 
        {
            int weight = -1;

            foreach (int[] edge in getAdjacentNodes(node)) 
            {     
                if(edge[1] == node2) weight = edge[2];
            }

            return weight;
        }
        
        public void printAllEdges()
        {
            // print all edges
            Console.WriteLine("Node 1" + " \t" + "Node 2" + " \t" + "Weight");

            foreach (int[] edge in getEdges()) Console.WriteLine(edge[0] + " \t" + edge[1] + " \t" + edge[2]);
        }

        public void printAdjacentEdges(int node)
        {
            // print all edges
            Console.WriteLine("Node 1" + " \t" + "Node 2" + " \t" + "Weight");
            foreach (int[] edge in getAdjacentNodes(node)) Console.WriteLine(edge[0] + " \t" + edge[1] + " \t" + edge[2]);
        }

        public void printAllTerminals()
        {
            // print all terminals
            foreach (int terminal in getTerminals())  Console.WriteLine(terminal);
        }

        private Tuple<int[], int[]> findSortestPath(int startNode) 
        {
            // Initialisierung of needed Lists
            List<int> nodesUnvisited = new List<int>(this.getNodes());

            // Arrays for storing distances and predecessors
            int nodesCount = nodesUnvisited.Count;
            int[] distances = new int[nodesCount + 1];
            int[] predecessors = new int[nodesCount + 1];

            // initialize distances with +infintiy 
            for (int i = 1; i < distances.Length; i++) distances[i] = int.MaxValue;

            // initial distance for starting node 
            distances[startNode] = 0;
            predecessors[startNode] = startNode;
            
            // init variables for loops
            int currentNode = startNode;
            int currentNodeDistance;
            List<int[]> adjacentNodes;

            // As long as there are unvisited nodes
            while (nodesUnvisited.Count > 0)
            {
                // distance of current node
                currentNodeDistance = distances[currentNode];

                // find all adjacent nodes
                adjacentNodes = this.getAdjacentNodes(currentNode);

                // init variables for inner loop
                int adjacentNodeID;
                int adjacentNodeDistance;
                int adjacentNodeDistanceNew;
                int adjacentNodeCost;

                // go through all adjacent nodes
                foreach (int[] adjacentNode in adjacentNodes)
                {
                    // store informations about the adjacent node
                    adjacentNodeID = adjacentNode[1];
                    adjacentNodeCost = adjacentNode[2];
                    adjacentNodeDistance = distances[adjacentNodeID];

                    // calculate new distance
                    adjacentNodeDistanceNew = currentNodeDistance + adjacentNodeCost;

                    // if th new distance is smaller than the old one
                    if(adjacentNodeDistanceNew < adjacentNodeDistance) 
                    {
                        // update distance and predecessors
                        distances[adjacentNodeID] = adjacentNodeDistanceNew;
                        predecessors[adjacentNodeID] = currentNode;
                    }
                }

                // mark current node as visited
                nodesUnvisited.Remove(currentNode);
               
                // init variables to determine next node
                int smallestDistance = int.MaxValue;
                int nextNode = -1; 

                // find node with smallest distance out of all unvisited nodes
                foreach (int nodeUnvisited in nodesUnvisited)
                {
                    // remember shortest adjacent distance and unvisited
                    if(smallestDistance >= distances[nodeUnvisited]) 
                    {
                        smallestDistance =  distances[nodeUnvisited];
                        nextNode = nodeUnvisited;
                    }
                }

                // break loop when there are unreachable nodes
                if(nextNode == -1) break; 

                // node with smallest distance as next node
                currentNode = nextNode;
    
            }
            return Tuple.Create(distances, predecessors); 
        }

        public void calculateDistanceMatrixTerminals(ParallelOptions options) {
            
            List<int> terminals = getTerminals().ToList();

            int n = getNodes().Max();
            distanceMatrix = new int[n+1, n+1];
            predecessorMatrix = new int[n+1, n+1];

            // compute dijkstra for all nodes as root node
            Parallel.ForEach(terminals, options, i  => 
            {                
                // compute dijkstra to find shortest path
                (int[] distances, int[] predecessors) = findSortestPath(i);
                
                // store distances for this node in the matrix
                for (int j = 1; j <= n; j++ ) distanceMatrix[i, j] = distances[j];

                // store predecessors for this node in the matrix
                for (int j = 1; j <= n; j++ ) predecessorMatrix[i, j] = predecessors[j];
            });
        }
        public void calculateDistanceMatrix(ParallelOptions options)
        {
            int n = getNodes().Count;
            distanceMatrix = new int[n+1, n+1];
            predecessorMatrix = new int[n+1, n+1];

            // compute dijkstra for all nodes as root node
            Parallel.ForEach(getNodes(), options, i  => 
            {                
                // compute dijkstra to find shortest path
                (int[] distances, int[] predecessors) = findSortestPath(i);
                
                // store distances for this node in the matrix
                for (int j = 1; j <= n; j++ ) distanceMatrix[i, j] = distances[j];

                // store predecessors for this node in the matrix
                for (int j = 1; j <= n; j++ ) predecessorMatrix[i, j] = predecessors[j];

            });

        }
    
        public List<int[]> solveMinimumSpanningTree() {

            // init variables
            List<int> terminals = this.getTerminals();
            List<int> nodes = this.getNodes();
            
            // reset length
            int lengthMST = 0;
            
            // choose starting node from set of terminals
            int startingNode = terminals[0];

            // Init Lists 
            List<int[]> edgesInMst = new List<int[]>(); // List of all edges in the final MST tree
            List<int> nodesUnvisited = nodes.ToList(); // List if unvisited nodes
            int[] distances = new int[nodes.Max() + 1]; // array with all distances
            int[] predecessors = new int[nodes.Max() + 1]; // array with all predecessors

            // initialize distances with +infintiy 
            for ( int i = 1; i < distances.Length;i++ ) distances[i] = int.MaxValue;

            // set distance for starting node to 0
            distances[startingNode] = 0;
            predecessors[startingNode] = -1;

            // Init variables for loop
            int nearestNode = -1;
            int shortestDistance = int.MaxValue;
            List<int[]> adjacentNodes = new List<int[]>();
            int adjacentNodeId;
            int adjacentNodeCost;

            // als long as there are unvisited nodes
            while(nodesUnvisited.Count > 0) 
            {
                // reset shortest distance
                shortestDistance = int.MaxValue;   

                // find Node with minimal distance
                foreach(int nodeUnvisited in nodesUnvisited) 
                {
                    if (shortestDistance > distances[nodeUnvisited]) 
                    {
                        shortestDistance = distances[nodeUnvisited];
                        nearestNode = nodeUnvisited;
                    }
                }
                
                // remove current node from list of unvisited ndoes
                nodesUnvisited.Remove(nearestNode);

                // add egde to mst if previous node is not emty
                if (predecessors[nearestNode] != -1) edgesInMst.Add(new int[] {predecessors[nearestNode], nearestNode, shortestDistance});

                // add current edge cost du treesize
                lengthMST = lengthMST + shortestDistance;

                // find adjacent nodes
                adjacentNodes = this.getAdjacentNodes(nearestNode); 

                foreach(int[] adjacentNode in adjacentNodes) 
                {
                    // get Node ID and cost
                    adjacentNodeId = adjacentNode[1];
                    adjacentNodeCost = adjacentNode[2];

                    if (nodesUnvisited.Contains(adjacentNodeId) & distances[adjacentNodeId] > adjacentNodeCost) 
                    {
                        distances[adjacentNodeId] = adjacentNodeCost;
                        predecessors[adjacentNodeId] = nearestNode;
                    }
                }
            }

            createGraphFromEdgeList(edgesInMst, terminals); 

            return edgesInMst;
        }

        public List<int[]> pruneTree() {
            
            //init variables
            List<int> terminals = this.getTerminals();
            int countLeafsFound = int.MaxValue;
            List<int[]> edgesInMst = this.getEdges().ToList();;

            // prune until no leafes found
            while (countLeafsFound > 0) 
            {
                countLeafsFound = 0;

                foreach(int[] edge in edges) 
                {
                    int fromNode = edge[0]; 
                    int toNode = edge[1];
                    int fromNodeDegree = 0;
                    int toNodeDegree = 0;

                    // search for adjacent edges for from node in mst 
                    foreach(int[] adjacentEdge in edgesInMst) if(adjacentEdge[0] == fromNode | adjacentEdge[1] == fromNode) fromNodeDegree++; 

                    // search for adjacent adges for the to node is not necessary if the from node is alredy a leafe
                    if (fromNodeDegree > 1) 
                    {
                        // search for adjacent edges for to node in mst 
                        foreach(int[] adjacentEdge in edgesInMst) if(adjacentEdge[0] == toNode | adjacentEdge[1] == toNode) toNodeDegree++; 

                    }

                    // remove edge from mst if one node is a leaf and not a terminal (CAN BE OPTIMIZED)
                    if(fromNodeDegree == 1 & !terminals.Contains(fromNode)) 
                    {
                        edgesInMst.Remove(edge);
                        countLeafsFound++;
                    }
                    
                    if(toNodeDegree == 1 & !terminals.Contains(toNode)) 
                    {
                        edgesInMst.Remove(edge);
                        countLeafsFound++;
                    }
                }
            }       

            createGraphFromEdgeList(edgesInMst, terminals); 

            return this.getEdges();    

        }
    }
}
