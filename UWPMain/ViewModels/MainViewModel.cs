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
            get => _IsRunning;
            private set
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
                    DelayedMutationRate = value;
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
                    DelayedCrossoverRate = value;
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
                    DelayedTournamentSize = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _DelayedTournamentSize;
        public int DelayedTournamentSize
        {
            get => _DelayedTournamentSize; set => Set(ref _DelayedTournamentSize, value);
        }

        private int _DelayedMutationRate;
        public int DelayedMutationRate { get => _DelayedMutationRate; set => Set(ref _DelayedMutationRate, value); }

        private int _DelayedCrossoverRate;
        public int DelayedCrossoverRate { get => _DelayedCrossoverRate; set => Set(ref _DelayedCrossoverRate, value); }

        public string CrossoverOperator
        {
            get
            {
                if (GeneticAlgorithm.CrossoverOperator == Algorithms.CrossoverOperator.CycleCrossover)
                    return "Cycle Crossover";

                return "Default";
            }
            set
            {
                switch (value)
                {
                    case "Cycle Crossover":
                        GeneticAlgorithm.CrossoverOperator = Algorithms.CrossoverOperator.CycleCrossover;
                        break;

                    default:
                        GeneticAlgorithm.CrossoverOperator = Algorithms.CrossoverOperator.Default;
                        break;
                }
                OnPropertyChanged();
            }
        }

        private string _DelayedCrossoverOperator;
        public string DelayedCrossoverOperator { get => _DelayedCrossoverOperator; set => Set(ref _DelayedCrossoverOperator, value); }

        private string _BenchmarkName;
        public string BenchmarkName { get => _BenchmarkName; set => Set(ref _BenchmarkName, value); }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainViewModel()
        {
            Generations = new ObservableCollection<GenerationViewModel>();
        }

        public async Task BeginExecutionAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => { IsRunning = true; Generations.Clear(); });
            bool IsSymmetric = true;
            switch (BenchmarkName)
            {
                case "ftv33":
                case "kro124p":
                case "rbg443":
                    IsSymmetric = false;
                    break;
            }
            GeneticAlgorithm.Input = await ReadBenchmarkFileAsync(BenchmarkName + ".txt", IsSymmetric);
            await Task.Run(() => GeneticAlgorithm.InitializePopulation(PopulationSize));
            await Task.Run(() => GeneticAlgorithm.EvolvePopulation());
            await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
            {
                CurrentGenerations = GeneticAlgorithm.Generations;
                BestDistance = GeneticAlgorithm.CurrentPopulation.Fittest.Distance;
            });

            await ResumeExecutionAsync();

            await DispatcherHelper.ExecuteOnUIThreadAsync(() => IsRunning = false);
        }

        public async Task ResumeExecutionAsync()
        {
            await DispatcherHelper.ExecuteOnUIThreadAsync(() => IsRunning = true);

            for (int i = CurrentGenerations; i < TotalGenerations; ++i)
            {
                if (IsStopped)
                    return;

                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    Generations.Insert(0, new GenerationViewModel() { FittestPath = GeneticAlgorithm.CurrentPopulation.Fittest, GenerationNumber = i });
                    //Generations.Insert(0, GeneticAlgorithm.CurrentPopulation.Fittest);
                    int Distance = GeneticAlgorithm.CurrentPopulation.Fittest.Distance;

                    if (Distance < BestDistance)
                        BestDistance = Distance;
                });

                await Task.Run(() => GeneticAlgorithm.EvolvePopulation());
                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    CurrentGenerations = GeneticAlgorithm.Generations;
                    TournamentSize = DelayedTournamentSize;
                    MutationRate = DelayedMutationRate;
                    CrossoverRate = DelayedCrossoverRate;
                    CrossoverOperator = DelayedCrossoverOperator;
                });
            }
        }

        public void PauseExecution() => IsRunning = false;

        static async Task<Graph> ReadBenchmarkFileAsync(string BenchmarkName, bool IsSymmetric = true)
        {
            Windows.Storage.StorageFolder InstalledLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
            string FilePath = InstalledLocation.Path + "\\Benchmark Files";
            Windows.Storage.StorageFolder FileFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(FilePath);
            Windows.Storage.StorageFile BenchmarkFile = await FileFolder.GetFileAsync(BenchmarkName);
            IEnumerable<string> Lines = await Windows.Storage.FileIO.ReadLinesAsync(BenchmarkFile);

            int Size = 0;
            int DiagonalSize = 0;
            int[] RawInput = null;
            int i = 0;
            bool DataLines = false;
            int NonDataLinesCounter = 0;

            // Regex instance to represent 1 or more whitespace occurrences.
            // Used as a variable here to avoid initialization overhead on every iteration.
            Regex RegexWhitespaces = new Regex(@"\s+");

            foreach (string Line in Lines)
            {
                // Skip lines until data section starts.
                if (Line == "EDGE_WEIGHT_SECTION")
                { DataLines = true; ++i; NonDataLinesCounter = i; continue; }

                if (!DataLines)
                {
                    if (i == 3) // Get no. of vertices/cities from the input file.
                    {
                        try { Size = Convert.ToInt32(Line.Replace("DIMENSION: ", "")); }
                        catch (Exception)
                        { Size = Convert.ToInt32(Line.Replace("DIMENSION : ", "")); }

                        if (IsSymmetric)
                        {
                            DiagonalSize = ((int)(Math.Pow(Size, 2) - Size) / 2) + Size;        // (Size^2 - Size / 2) + Size
                            RawInput = new int[DiagonalSize];
                        }

                        else
                            RawInput = new int[Size * Size];
                    }
                    ++i;
                }

                else if (Line.ToUpper().Contains("EOF") || Line.Contains(EOF) || Line.Contains("DISPLAY_DATA_SECTION"))
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
                            RawInput[i - NonDataLinesCounter] = Convert.ToInt32(Value);
                            ++i;
                        }
                    }
                }
            }

            Graph Input = new Graph(Size);
            int k = 0;
            if (IsSymmetric)
            {
                for (i = 0; i < Size; ++i)  // i runs for number of nodes/cities
                {
                    for (int j = 0; j < i + 1; ++j)             // j runs until diagonal is reached for each node [inclusive].
                        Input.InsertEdge(j, i, RawInput[k++]);  // j = row, i = column
                }
            }

            else
            {
                for (i = 0; i < Size; ++i)
                {
                    for (int j = 0; j < Size; ++j)
                        Input.InsertEdge(i, j, RawInput[k++]);
                }
            }

            return Input;
        }
    }
}
