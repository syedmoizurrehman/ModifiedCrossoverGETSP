using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Algorithms;
using DataStructures;
using Microsoft.Toolkit.Uwp.Helpers;

namespace UWPMain.ViewModels
{
    public class MainViewModel : BindableBase
    {
        private const char EOF = '\uffff';

        public bool IsStopped { get => !_IsRunning; }

        public ObservableCollection<GenerationViewModel> Generations { get; set; }

        private bool _IsRunning;
        public bool IsRunning
        {
            get => _IsRunning; set
            {
                if (value != _IsRunning)
                {
                    _IsRunning = value;
                    OnPropertyChanged(nameof(IsRunning));
                    OnPropertyChanged(nameof(IsStopped));
                }
            }
        }

        private int _PopulationSize;
        /// <summary>
        /// Size of population to be used when running the genetic algorithm. Cannot be set after algorithm has begun execution.
        /// </summary>
        public int PopulationSize
        {
            get => _PopulationSize;
            set
            {
                if (!IsRunning)
                    Set(ref _PopulationSize, value);
            }
        }

        private int _BestDistance;
        public int BestDistance
        {
            get => _BestDistance;
            set => Set(ref _BestDistance, value);
        }

        private int _TotalGenerations;
        public int TotalGenerations
        {
            get => _TotalGenerations;
            set
            {
                if (!IsRunning)
                    Set(ref _TotalGenerations, value);
            }
        }

        public int _CurrentGenerations;
        public int CurrentGenerations { get => _CurrentGenerations; set => Set(ref _CurrentGenerations, value); }

        public int MutationRate
        {
            get => (int)(GeneticAlgorithm.MutationRate * 100);
            set
            {
                if (value != MutationRate)
                {
                    GeneticAlgorithm.MutationRate = (double)value / 100;
                    OnPropertyChanged();
                }
            }
        }

        public int CrossoverRate
        {
            get => (int)(GeneticAlgorithm.CrossoverRate * 100);
            set
            {
                if (value != CrossoverRate)
                {
                    GeneticAlgorithm.CrossoverRate = (double)value / 100;
                    OnPropertyChanged();
                }
            }
        }

        public int TournamentSize
        {
            get => GeneticAlgorithm.TournamentSize;
            set
            {
                if (value != TournamentSize)
                {
                    GeneticAlgorithm.TournamentSize = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Generations = new ObservableCollection<GenerationViewModel>();
        }

        public async Task BeginExecutionAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => IsRunning = true);

            GeneticAlgorithm.Input = await ReadBenchmarkFile("gr21.txt");
            await Task.Run(() => GeneticAlgorithm.InitializePopulation(PopulationSize));
            await Task.Run(() => GeneticAlgorithm.EvolvePopulation());
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => CurrentGenerations = GeneticAlgorithm.Generations);

            for (int i = 0; i < TotalGenerations - 1; ++i)
            {
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => 
                {
                    Generations.Insert(0, new GenerationViewModel() { FittestPath = GeneticAlgorithm.CurrentPopulation.Fittest, GenerationNumber = i });
                    //Generations.Insert(0, GeneticAlgorithm.CurrentPopulation.Fittest);
                    int Distance = GeneticAlgorithm.CurrentPopulation.Fittest.Distance;

                    if (Distance < BestDistance)
                        BestDistance = Distance;
                });

                await Task.Run(() => GeneticAlgorithm.EvolvePopulation());
                await DispatcherHelper.ExecuteOnUIThreadAsync(() => CurrentGenerations = GeneticAlgorithm.Generations);
            }

            await DispatcherHelper.ExecuteOnUIThreadAsync(() => IsRunning = false);
        }

        private async Task<Graph> ReadBenchmarkFile(string BenchmarkName)
        {
            Windows.Storage.StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
            string FilePath = installedLocation.Path + "\\Benchmark Files";
            Windows.Storage.StorageFolder FileFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(FilePath);
            Windows.Storage.StorageFile BenchmarkFile = await FileFolder.GetFileAsync(BenchmarkName);
            IEnumerable<string> Lines = await Windows.Storage.FileIO.ReadLinesAsync(BenchmarkFile);

            //IEnumerable<string> Lines = File.ReadLines(Directory.GetCurrentDirectory().Replace("\\bin\\Debug", @"\Benchmark Files\" + BenchmarkName));
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
    }
}
