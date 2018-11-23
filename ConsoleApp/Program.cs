using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Algorithms;
using DataStructures;
using Extensions;

namespace ConsoleApp
{
    class Program
    {
        const char EOF = '\uffff';

        static Graph ReadBenchmarkFile(string BenchmarkName)
        {
            IEnumerable<string> Lines = File.ReadLines(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", @"\Benchmark Files\" + BenchmarkName));
            int Size = 0;
            int DiagonalSize = 0;
            int[] RawInput = null;
            int i = 0;

            // Regex instance to represent 1 or more whitespace occurrences.
            // Used as a variable here to avoid initialization overhead on every iteration.
            Regex RegexWhitespaces = new Regex(@"\s+");

            foreach (string Line in Lines)
            {
                // Skip first seven lines.
                if (i < 7)
                {
                    if (i == 3) // Get no. of vertices/cities from the input file.
                    {
                        Size = Convert.ToInt32(Line.Replace("DIMENSION: ", ""));
                        DiagonalSize = ((int)(Math.Pow(Size, 2) - Size) / 2) + Size;        // (Size^2 - Size / 2) + Size
                        RawInput = new int[DiagonalSize];
                    }
                    ++i;
                }

                else if (Line.ToUpper().Contains("EOF") || Line.Contains(EOF))
                    break;

                else
                {
                    string[] Values = new string[1] { Line };
                    
                    // Check if Line has 1 or more occurrences of whitespace characters.
                    if (RegexWhitespaces.Matches(Line).Count > 0)
                        Values = RegexWhitespaces.Split(Line);                         // Split Line into array of strings upon each occurrence of whitespace characters.

                    foreach (string Value in Values)
                    {
                        if (Value != string.Empty)
                        {
                            RawInput[i - 7] = Convert.ToInt32(Value);
                            ++i;
                        }
                    }
                }
            }

            Graph Input = new Graph(Size);

            /*
            A 3x3 Graph in adjacency matrix form given below,
            *   0    1   2
            0   0    a   b
            1   a    0   c
            2   b    c   0
             
            is represented as
            
            0   a   0   b   c   0 
            */

            int k = 0;
            for (i = 0; i < Size; ++i)  // i runs for number of nodes/cities
            {
                for (int j = 0; j < i + 1; ++j) // j runs until diagonal is reached for each node [inclusive].
                    Input.InsertEdge(j, i, RawInput[k++]);  // j = row, i = column
            }

            return Input;
        }

        static async Task Main(string[] args)
        {
            GeneticAlgorithm.Input = ReadBenchmarkFile("gr21.tsp");
            GeneticAlgorithm.TournamentSize = 40;
            GeneticAlgorithm.MutationRate = 0.1;
            GeneticAlgorithm.InitializePopulation(size: 150);
            int Best = GeneticAlgorithm.CurrentPopulation.Fittest.Distance;
            int Worst = GeneticAlgorithm.CurrentPopulation.Fittest.Distance;
            GeneticAlgorithm.EvolvePopulation();
            for (int i = 0; i < 500; ++i)
            {
                //await Task.Delay(1000);
                Console.WriteLine("Generation: " + GeneticAlgorithm.Generations);

                #region Debug
#if DebugInfo

                Console.WriteLine($"Tournament selection selected {GeneticAlgorithm.SameParentsSelected} same pair(s) of parents.");
                Console.WriteLine($"Crossover produced {GeneticAlgorithm.ChildIsFirstParent} child(ren) out of {GeneticAlgorithm.CurrentPopulation.Count} which were exact copy of first parent");
                Console.WriteLine($"Crossover produced {GeneticAlgorithm.ChildIsSecondParent} child(ren) out of {GeneticAlgorithm.CurrentPopulation.Count} which were exact copy of second parent");
#endif
                #endregion

                int Distance = GeneticAlgorithm.CurrentPopulation.Fittest.Distance;
                Console.WriteLine(GeneticAlgorithm.CurrentPopulation.Fittest + " => " + Distance + "\n");
                if (Distance < Best)
                    Best = Distance;

                Console.WriteLine("Change2");

                if (Distance > Worst)
                    Worst = Distance;

                Console.WriteLine("Best Yet = " + Best + "\n");
                Console.WriteLine("Worst Yet = " + Worst + "\n");
                GeneticAlgorithm.EvolvePopulation();
            }
            Console.WriteLine("Done");
        }
    }
}
