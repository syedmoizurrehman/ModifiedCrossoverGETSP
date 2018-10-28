using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DataStructures;

namespace ConsoleApp
{
    class Program
    {
        const char EOF = '\uffff';

        static Graph ReadInputLines(string BenchmarkName)
        {
            var Lines = File.ReadLines(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", @"\Benchmark Files\" + BenchmarkName));
            int Size = 0;
            int DiagonalSize = 0;
            int[] RawInput = null;
            int i = 0;
            foreach (var Line in Lines)
            {
                // Skip first seven lines.
                if (i < 7)
                {
                    if (i == 3) // Get Dimensions from the input file.
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
                    Regex RegexWhitespaces = new Regex(@"\s+");
                    if (RegexWhitespaces.Matches(Line).Count > 0)
                    {
                        string[] Values = RegexWhitespaces.Replace(Line, " ").Split(' ');
                        foreach (var Value in Values)
                        {
                            if (Value != string.Empty)
                            {
                                RawInput[i - 7] = Convert.ToInt32(Value);
                                ++i;
                            }
                        }
                    }

                    else
                    {
                        RawInput[i - 7] = Convert.ToInt32(Line);
                        ++i;
                    }
                }
            }

            Graph Input = new Graph(Size);
            Console.WriteLine(Input);

            int k = 0;
            /*
            A 3x3 Graph in adjacency matrix form given below,
            *   0    1   2
            0   0    a   b
            1   a    0   c
            2   b    c   0
             
            is represented as
            
            0   a   0   b   c   0 
            */

            for (i = 0; i < Size; ++i)  // i runs for number of nodes/cities
            {
                for (int j = 0; j < i + 1; ++j) // j runs until diagonal is reached for each node [inclusive].
                    Input.InsertEdge(j, i, RawInput[k++]);  // j = row, i = column
            }

            return Input;
        }

        static void Main(string[] args)
        {
            Graph Input = ReadInputLines("Test1.tsp");

            Console.WriteLine("\n" + Input.ToMatrixString());
            
        }
    }
}
