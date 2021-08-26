 using System;
 using System.Collections.Generic;
 using System.Collections;
 using System.Linq;
 using System.Threading.Tasks;


 namespace SteinerTreeProblem
 {
     class SteinerHeuristic
     {

        private Graph graph;
        private ParallelOptions options;
        int length;

        public int getLength()
        {
            return this.length;
        }


         public SteinerHeuristic(Graph graph, ParallelOptions options)
        {
            this.graph = graph;
            this.options = options;
            this.length = 0;
        }

         public void approxMinimumSteinerTree() 
         {
            List<int[]> edges = graph.getEdges();
            List<int> terminals = graph.getTerminals();
            List<int> nodes = graph.getNodes();

            // Determine pruned MST using Prims Mimumim Spanning Tree Algorithm
            SteinerTreeKMBApprox kmbApprox = new SteinerTreeKMBApprox(graph, options);
            List<int[]> edgesInMst = kmbApprox.approxMinimumSteinerTree();
            Graph graphMst = new Graph(edgesInMst, terminals);

            Console.WriteLine("Approx Steiner Tree Length = " + kmbApprox.getLength());

            // Determine nodes not in MST
            List<int> nodesToAdd = nodes.Except(graphMst.getNodes()).ToList() ;

            // list of steiner nodes
            List<int> steinerNodes = new List<int>();

            // initial length
            int shortestsLength = kmbApprox.getLength();
            int bestCandidate = 0;
            bool candidateFound = true;

            // While nodes can be added and improvement threshold can be meet
            while(candidateFound) 
            {
                candidateFound = false;

                foreach(int nodeToAdd in nodesToAdd) {

                    List<int> newTerminsals = terminals.Concat(steinerNodes).ToList();
                    newTerminsals.Add(nodeToAdd); 

                    kmbApprox.approxMinimumSteinerTree(newTerminsals);

                    if(shortestsLength > kmbApprox.getLength()) {
                        shortestsLength = kmbApprox.getLength();
                        Console.WriteLine("\t Potential Steiner Node found " + nodeToAdd + " => Approx Steiner Tree Length = " + shortestsLength);
                        bestCandidate = nodeToAdd;
                        candidateFound = true;
                    }

                }

                if(candidateFound) {
                    Console.WriteLine("New Steiner Node added " + bestCandidate + " => Approx Steiner Tree Length = " + shortestsLength);
                    steinerNodes.Add(bestCandidate);
                    nodesToAdd.Remove(bestCandidate);
                }
            }

            length = shortestsLength;  
         }
     }
 }
