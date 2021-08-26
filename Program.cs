using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Text;

namespace SteinerTreeProblem
{
    class SteinerTreeProblem
    {
        static void Main(string[] args)
        {
           
            Console.WriteLine("\n##################################");
            Console.WriteLine("# Testcase: Steiner Tree Problem #");
            Console.WriteLine("##################################");

            // Choose Instace
            Console.WriteLine("\nPlease enter the Instace ID you want to use (e.g \"001\", \"002\", ..., \"199\"):");
            string instance = Console.ReadLine();

            if(instance.Length == 0) instance = "001";
          
            // choose Algorithm
            Console.WriteLine("\nPlease choose a algorithm:\n");
            Console.WriteLine("\t 1 - Dreyfus Wagner");
            Console.WriteLine("\t 2 - Approximation according to Kou, Markowsky and Berman");
            Console.WriteLine("\t 3 - Approximation according to Mehlhorn");
            Console.WriteLine("\t 4 - 1-Steiner Heuristic");
            int algo = Convert.ToInt32(Console.ReadLine());
           

            Console.WriteLine("\nNumber of CPU Cores do you want to use:\n");
            int cores = Convert.ToInt32(Console.ReadLine());         

            // Parameter
            string inputFilePath = "./data/instance" + instance + ".gr";
            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = cores; // -1 is for unlimited. 1 is for sequential.

            // starting timer
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // create Graph
            Graph graph = new Graph(inputFilePath);


            if(algo == 2) 
            {
                // create Instance of KMB Steiner Tree Approximation
                SteinerTreeKMBApprox kmbApprox = new SteinerTreeKMBApprox(graph, options);
                List<int[]> edges_in_approx = kmbApprox.approxMinimumSteinerTree();
                Console.WriteLine("Approx Steiner Tree Length = " + kmbApprox.getLength());

                Console.WriteLine("\nEdges in Steiner Tree:\n");
                foreach(int[] edge in edges_in_approx) {
                    Console.WriteLine(edge[0] + " ---- (" + edge[2] + ") ---> " + edge[1] );
                }
            }
            else if (algo == 3) 
            {
                //create Instance of Mehlhorns Steiner Tree Approximation
                SteinerTreeMehlhornApprox mehlhornApprox = new SteinerTreeMehlhornApprox(graph, options);
                List<int[]> edges_in_approx =  mehlhornApprox.approxMinimumSteinerTree();
                Console.WriteLine("Approx Steiner Tree Length = " + mehlhornApprox.getLength());

                Console.WriteLine("\nEdges in Steiner Tree:\n");
                foreach(int[] edge in edges_in_approx) {
                    Console.WriteLine(edge[0] + " ---- (" + edge[2] + ") ---> " + edge[1] );
                }
            }
            else if (algo == 4) 
            {
                // create Instance of 1-Steiner Heuristic
                SteinerHeuristic steinerHeuristic = new SteinerHeuristic(graph, options);
                steinerHeuristic.approxMinimumSteinerTree();
            }
            else 
            {
                // create Instance of Dreyfus Wagner Algorithm
                SteinerTreeSolverParallel steinerTreeSolver = new SteinerTreeSolverParallel(graph, options);
                Tuple<List<int[]>, int> result = steinerTreeSolver.solveMinimumSteinerTree();

                Console.WriteLine("\nEdges in Steiner Tree:\n");
                foreach(int[] edge in result.Item1) {
                    Console.WriteLine(edge[0] + " ---- (" + edge[2] + ") ---> " + edge[1] );
                }
            }

            // stopping Timer
            stopWatch.Stop();
            Console.WriteLine("------------------------------------------------------------");
            Console.WriteLine("Runtime: {0}" , stopWatch.Elapsed);

            //testDW();
            //testKMB();
            //testMehlhorn();
            //testHeuristic();
            //Console.WriteLine("Terminals = " + String.Join(",", graph.getTerminals()));
            //Console.WriteLine("Found minimal Length = " + result.Item2);

            //Console.WriteLine(Math.Sqrt(graph.getDistanceMatrix().LongLength));

            //SaveArrayAsCSV(graph.getDistanceMatrix(), "./distance_matrix_009.csv");
            //SaveListAsCSV(result.Item1, "./steiner_tree_009.csv");
            //SaveListAsCSV(edges_in_approx, "./steiner_tree_approx_mehlhorn_009.csv");
            //SaveListAsCSV(edges_in_approx, "./steiner_tree_approx_kmb_007.csv");

        }

        public static void testMehlhorn() {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8; // -1 is for unlimited. 1 is for sequential.
            DirectoryInfo d = new DirectoryInfo("./data/");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.gr"); //Getting Text files
            String outputFile = "./csv/results_mehlhorn.csv";


            // write csv header
            var csv = new StringBuilder();
            var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                                "filename", "count_terminals", "count_nodes", "count_edges", 
                                "approx_tree_length", "runtime_in_seconds");

            csv.AppendLine(newLine);
            File.AppendAllText(outputFile, csv.ToString());  

            foreach(FileInfo file in Files )
            {
                try
                {
                    bool Completed = ExecuteWithTimeLimit(TimeSpan.FromMinutes(1), () =>
                    {
                        // starting timer
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        // create Graph
                        Graph graph = new Graph(file.ToString());
                        SteinerTreeMehlhornApprox mehlhornApprox = new SteinerTreeMehlhornApprox(graph, options);
                        mehlhornApprox.approxMinimumSteinerTree();
                    
                        stopWatch.Stop();

                        var csv = new StringBuilder();
                        var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                                            file.Name, graph.getTerminals().Count, graph.getNodes().Count, graph.getEdges().Count, 
                                            mehlhornApprox.getLength(), stopWatch.Elapsed.TotalSeconds.ToString());
                        File.AppendAllText(outputFile, csv.AppendLine(newLine).ToString());  

                        var info = string.Format("Instance {0} (|T| = {1}, |N| = {2}, |E| = {3}), Length = {4}, Runtime = {5}", 
                            file.Name, graph.getTerminals().Count, graph.getNodes().Count, graph.getEdges().Count, 
                        mehlhornApprox.getLength(), stopWatch.Elapsed.TotalSeconds.ToString());
                        Console.WriteLine(info);

                        });

                    if(!Completed) Console.WriteLine("Timelimit reached!");
                }
                catch (InvalidCastException e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void testKMB() {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8; // -1 is for unlimited. 1 is for sequential.
            DirectoryInfo d = new DirectoryInfo("./data/");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.gr"); //Getting Text files
            String outputFile = "./csv/results_kmb.csv";


            // write csv header
            var csv = new StringBuilder();
            var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                                "filename", "count_terminals", "count_nodes", "count_edges", 
                                "approx_tree_length", "runtime_in_seconds");

            csv.AppendLine(newLine);
            File.AppendAllText(outputFile, csv.ToString());  

            foreach(FileInfo file in Files )
            {
                try
                {
                    bool Completed = ExecuteWithTimeLimit(TimeSpan.FromMinutes(1), () =>
                    {
                        // starting timer
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        // create Graph
                        Graph graph = new Graph(file.ToString());
                        SteinerTreeKMBApprox steinerTreeKMBApprox = new SteinerTreeKMBApprox(graph, options);
                        steinerTreeKMBApprox.approxMinimumSteinerTree();
                    
                        stopWatch.Stop();

                        var csv = new StringBuilder();
                        var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                                            file.Name, graph.getTerminals().Count, graph.getNodes().Count, graph.getEdges().Count, 
                                            steinerTreeKMBApprox.getLength(), stopWatch.Elapsed.TotalSeconds.ToString());
                        File.AppendAllText(outputFile, csv.AppendLine(newLine).ToString());  

                        var info = string.Format("Instance {0} (|T| = {1}, |N| = {2}, |E| = {3}), Length = {4}, Runtime = {5}", 
                            file.Name, graph.getTerminals().Count, graph.getNodes().Count, graph.getEdges().Count, 
                        steinerTreeKMBApprox.getLength(), stopWatch.Elapsed.TotalSeconds.ToString());
                        Console.WriteLine(info);

                        });

                    if(!Completed) Console.WriteLine("Timelimit reached!");
                }
                catch (InvalidCastException e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        
       public static void testHeuristic() {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8; // -1 is for unlimited. 1 is for sequential.
            DirectoryInfo d = new DirectoryInfo("./data/");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.gr"); //Getting Text files
            String outputFile = "./csv/results_heursitic.csv";


            // write csv header
            var csv = new StringBuilder();
            var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                                "filename", "count_terminals", "count_nodes", "count_edges", 
                                "approx_tree_length", "runtime_in_seconds");

            csv.AppendLine(newLine);
            File.AppendAllText(outputFile, csv.ToString());  

            foreach(FileInfo file in Files )
            {
                try
                {
                    bool Completed = ExecuteWithTimeLimit(TimeSpan.FromMinutes(7), () =>
                    {
                        // starting timer
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        // create Graph
                        Graph graph = new Graph(file.ToString());
                        SteinerHeuristic steinerHeuristic = new SteinerHeuristic(graph, options);
                        steinerHeuristic.approxMinimumSteinerTree();
                    
                        stopWatch.Stop();

                        var csv = new StringBuilder();
                        var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\",\"{3}\",\"{4}\",\"{5}\"", 
                                            file.Name, graph.getTerminals().Count, graph.getNodes().Count, graph.getEdges().Count, 
                                            steinerHeuristic.getLength(), stopWatch.Elapsed.TotalSeconds.ToString());
                        File.AppendAllText(outputFile, csv.AppendLine(newLine).ToString());  

                        var info = string.Format("Instance {0} (|T| = {1}, |N| = {2}, |E| = {3}), Length = {4}, Runtime = {5}", 
                            file.Name, graph.getTerminals().Count, graph.getNodes().Count, graph.getEdges().Count, 
                        steinerHeuristic.getLength(), stopWatch.Elapsed.TotalSeconds.ToString());
                        Console.WriteLine(info);

                        });

                    if(!Completed) Console.WriteLine("Timelimit reached!");
                }
                catch (InvalidCastException e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public static void testDW() {

            ParallelOptions options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 8; // -1 is for unlimited. 1 is for sequential.
            DirectoryInfo d = new DirectoryInfo("./data/");//Assuming Test is your Folder
            FileInfo[] Files = d.GetFiles("*.gr"); //Getting Text files
            String outputFile = "./csv/results_dw.csv";

            // write csv header
            var csv = new StringBuilder();
            var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\"",
                                "filename", "tree_length", "runtime_in_seconds");

            csv.AppendLine(newLine);
            File.AppendAllText(outputFile, csv.ToString());  

            foreach(FileInfo file in Files )
            {
                try
                {
                    bool Completed = ExecuteWithTimeLimit(TimeSpan.FromMinutes(3), () =>
                    {
                        // starting timer
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();

                        // create Graph
                        //Graph graph = new Graph(file.ToString());
                        Graph graph = new Graph(file.ToString());
                        SteinerTreeSolverParallel steinerTreeSolver = new SteinerTreeSolverParallel(graph, options);
                        Tuple<List<int[]>, int> result = steinerTreeSolver.solveMinimumSteinerTree();
                    
                        Console.WriteLine("Terminals = " + String.Join(",", graph.getTerminals()));
                        Console.WriteLine("Found minimal Length = " + result.Item2);

                        stopWatch.Stop();
                        Console.WriteLine("Time elapsed: {0}" , stopWatch.Elapsed);

                        var csv = new StringBuilder();
                        var newLine = string.Format("\"{0}\",\"{1}\",\"{2}\"", file.Name, result.Item2 , stopWatch.Elapsed.TotalSeconds.ToString());
                        csv.AppendLine(newLine);
                        File.AppendAllText(outputFile, csv.ToString());  
                        });

                    if(!Completed) Console.WriteLine("Timelimit reached!");
                }
                catch (Exception ex)
                {
                    //return null;
                    Console.WriteLine(ex);
                }

                //break;
            }
        }

        public static bool ExecuteWithTimeLimit(TimeSpan timeSpan, Action codeBlock)
        {   
            try
            {
                Task task = Task.Factory.StartNew(() => codeBlock());
                task.Wait(timeSpan);
                return task.IsCompleted;

                //Task task = new Task(codeBlock);
                //task.Wait(timeSpan); 
                //task.Start();
                //return task.IsCompleted;
            }
            catch (AggregateException ae)
            {
                throw ae.InnerExceptions[0];
            }   
        }

        private static void SaveArrayAsCSV(int[,] array, string file)
        {
            using (StreamWriter outfile = new StreamWriter(file))
            {

                int n = (int)Math.Sqrt(array.LongLength);

                for (int x = 1; x < n; x++)
                {
                    string content = "";
                    for (int y = 1; y < n; y++)
                    {
                        content += array[x, y].ToString() + ";";
                    }
                    outfile.WriteLine(content);
                }
            }
        }

        private static void SaveListAsCSV(List<int[]> list, string file)
        {
            using (StreamWriter outfile = new StreamWriter(file))
            {

                int n = list.Count;

                foreach (int[] item in list) {
                    
                    string content = String.Join(";", item);

                    outfile.WriteLine(content);
                }
            }
        }
    }
}
