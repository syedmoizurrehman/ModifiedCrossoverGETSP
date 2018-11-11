using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataStructures;
using Extensions;

namespace Algorithms
{
    public static class GeneticAlgorithm
    {
        /// <summary>
        /// Gets number of generations i.e. number of times CurrentPopulation has evolved.
        /// </summary>
        public static int Generations { get; private set; }
        public static Graph Input { get; set; }
        public static int TournamentSize;
        public static double MutationRate;
        public static int SourceIndex;
        public static int DestinationIndex;
        public static Population CurrentPopulation { get; private set; }

#if DebugInfo

        public static int SameParentsSelected;
        public static int ChildIsFirstParent;
        public static int ChildIsSecondParent;

#endif

        static GeneticAlgorithm()
        {
            Input = null;
            TournamentSize = 3;
            MutationRate = 0.5;
            Generations = 0;
        }

        public static void InitializePopulation(int size)
        {
            CurrentPopulation = new Population(Input, size);
        }

        /// <summary>
        /// Mutates a path indivudual according to <see cref="MutationRate"/>.
        /// </summary>
        /// <param name="pathIndividual">The path individual to mutate.</param>
        public static void Mutate(PathGraph pathIndividual)
        {
            // Iterate through all path vertices
            for (int Pos1 = 1; Pos1 < pathIndividual.Count - 1; ++Pos1)
            {
                // Apply mutation rate
                if (ThreadSafeRandom.CurrentThreadsRandom.NextDouble() < MutationRate)
                {
                    // Get a second random position in the path
                    int Pos2 = ThreadSafeRandom.CurrentThreadsRandom.Next(1, pathIndividual.Count - 2);

                    // Swap the vertices at target position in path
                    var Temp = pathIndividual[Pos1];
                    pathIndividual[Pos1] = pathIndividual[Pos2];
                    pathIndividual[Pos2] = Temp;
                }
            }
        }

        /// <summary>
        /// Breeds two parent path individuals to create an offspring path individual.
        /// </summary>
        /// <param name="firstParent">The first path individual to breed with second path individual .</param>
        /// <param name="secondParent">The second path individual to breed with first path individual.</param>
        /// <returns>Offspring path individual of specified parents.</returns>
        public static PathGraph Crossover(PathGraph firstParent, PathGraph secondParent)
        {
            int CircuitLength = Input.Size + 1;

            // Create new child path
            PathGraph ChildPath = new PathGraph(CircuitLength, Input);
            ChildPath[0] = Input[0];
            ChildPath[CircuitLength - 1] = Input[0];

            // Get random start and end indices positions for firstParent
            int StartPos = ThreadSafeRandom.CurrentThreadsRandom.Next(1, CircuitLength - 1);
            int EndPos = ThreadSafeRandom.CurrentThreadsRandom.Next(1, CircuitLength - 1) + ThreadSafeRandom.CurrentThreadsRandom.Next(1, CircuitLength - 1);

            // Loop and add the sub tour from firstParent to our child
            for (int i = 1; i < CircuitLength - 1; ++i)
            {
                // If our start position is less than the end position
                if (StartPos < EndPos && i > StartPos && i < EndPos)
                    ChildPath[i] = firstParent[i];
                
                // If our start position is larger
                else if (StartPos > EndPos)
                {
                    //if (!(i < StartPos && i > EndPos))
                    if (i >= StartPos || i <= EndPos )
                        ChildPath[i] = firstParent[i];
                }
            }

            // Loop through parent2's path
            for (int i = 1; i < CircuitLength - 1; ++i)
            {
                // If child doesn't have the city add it
                if (!ChildPath.Contains(secondParent[i]))
                {
                    // Loop to find a spare position in the child's tour
                    for (int j = 1; j < CircuitLength - 1; ++j)
                    {
                        // Spare position found, add city
                        if (ChildPath[j] == null)
                        {
                            ChildPath[j] = secondParent[i];
                            break;
                        }
                    }
                }
            }

            return ChildPath;
        }

        public static PathGraph CycleCrossover(PathGraph firstParent, PathGraph secondParent)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Proceeds to next generation by evolving the CurrentPopulation.
        /// </summary>
        /// <returns></returns>
        public static void EvolvePopulation()
        {
            // Create a new population of same size as CurrentPopulation.
            Population EvolvedPopulation = new Population(CurrentPopulation.Count);

#if DebugInfo

            SameParentsSelected = 0;
            ChildIsFirstParent = 0;
            ChildIsSecondParent = 0;
#endif
            // Breed offsprings to populate the evolved population.
            for (int i = 0; i < CurrentPopulation.Count; ++i)
            {
                // Select a random path individual from CurrentPopulation.
                PathGraph Parent1 = TournamentSelect();

                // Select another random path individual from CurrentPopulation.
                PathGraph Parent2 = TournamentSelect();

                // Apply crossover operator on the previously selected path individuals to breed a child path individual.
                PathGraph Child = Crossover(Parent1, Parent2);
                
                #region Debug information
#if DebugInfo

                if (Parent1 == Parent2)
                    ++SameParentsSelected;

                bool ParentSimilar = true;
                for (int j = 0; j < Child.Count; j++)
                {
                    if (Child[j] == Parent1[j])
                        continue;

                    else
                    {
                        ParentSimilar = false;
                        break;
                    }
                }

                if (ParentSimilar)
                    ++ChildIsFirstParent;

                else
                {
                    for (int j = 0; j < Child.Count; j++)
                    {
                        if (Child[j] == Parent2[j])
                            continue;

                        else
                        {
                            ParentSimilar = false;
                            break;
                        }
                    }
                }
                if (ParentSimilar)
                    ++ChildIsSecondParent;

#endif
                #endregion
                
                // Introduce randomness into the child according to the MutationRate.
                Mutate(Child);

                // Add the child to new population.
                EvolvedPopulation.Add(Child);
            }

            // Update CurrentPopulation.
            CurrentPopulation = EvolvedPopulation;
            ++Generations;
        }

        public static PathGraph CustomSelect()
        {
            int RandomIndex = new Random().Next(0, TournamentSize);
            CurrentPopulation.ShuffleSubset(RandomIndex, TournamentSize);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Selects a candidate path individual from CurrentPopulation for crossover.
        /// </summary>
        /// <returns></returns>
        public static PathGraph TournamentSelectNew()
        {
            // Create a new population of size TournamentSize.
            Population TournamentPopulation = new Population(Input, TournamentSize);

            // Fill the new population with random path individuals from CurrentPopulation.
            for (int i = 0; i < TournamentSize; i++)
            {
                // Get a random path individual from CurrentPopulation and add it to TournamentPopulation.
                int RandomIndex = new Random().Next(0, CurrentPopulation.Count);
                //TournamentPopulation[i] = CurrentPopulation[RandomIndex];
                TournamentPopulation.Insert(i, CurrentPopulation[RandomIndex]);
            }

            // Return the fittest path from new population.
            return TournamentPopulation.Fittest;
        }

        /// <summary>
        /// Selects a candidate path individual from CurrentPopulation for crossover.
        /// </summary>
        /// <returns></returns>
        public static PathGraph TournamentSelect()
        {
            // Create a new population of size TournamentSize.
            Population TournamentPopulation = new Population(TournamentSize);

            // Fill the new population with random path individuals from CurrentPopulation.
            for (int i = 0; i < TournamentSize; i++)
            {
                // Get a random path individual from CurrentPopulation and add it to TournamentPopulation.
                int RandomIndex = ThreadSafeRandom.CurrentThreadsRandom.Next(0, CurrentPopulation.Count);
                TournamentPopulation.Add(CurrentPopulation[RandomIndex]);
            }
            // Return the fittest path from new population.
            return TournamentPopulation.Fittest;
        }
    }

    /// <summary>
    /// This class represents a collection of <see cref="PathGraph"/> objects.
    /// </summary>
    public class Population : IList<PathGraph>
    {
        private List<PathGraph> _Individuals;

        /// <summary>
        /// Gets the fittest path individual in this <see cref="Population"/>. 
        /// O(Population Size * Graph Size) operation as it iterates each individual to compare its distance and the 
        /// distance of each individual is calculated in O(n).
        /// </summary>
        public PathGraph Fittest
        {
            get
            {
                PathGraph FittestIndividual = _Individuals[0];
                foreach (var PathIndividual in _Individuals)
                {
                    // Find minimum distance path.
                    if (PathIndividual.Distance < FittestIndividual.Distance)
                        FittestIndividual = PathIndividual;
                }
                return FittestIndividual;
            }
        }

        /// <summary>
        /// Initialized a new instance of <see cref="Population"/> class with no individuals.
        /// </summary>
        /// <param name="capacity">The max. number of path individuals this population can contain.</param>
        public Population(int capacity) => _Individuals = new List<PathGraph>(capacity);

        /// <summary>
        /// Creats an instance of <see cref="Population"/> class and generates individuals in it.
        /// </summary>
        /// <param name="inputGraph">The input data set of cities.</param>
        /// <param name="capacity">Size of population i.e. number of individuals.</param>
        public Population(Graph inputGraph, int capacity)
        {
            // Initialize individuals.
            _Individuals = new List<PathGraph>(capacity);

            GenerateIndividuals(inputGraph);
        }

        /// <summary>
        /// Populates the collection of individuals by creating new individuals from specified graph.
        /// </summary>
        /// <param name="inputGraph">The graph to generate random path individuals from.</param>
        private void GenerateIndividuals(Graph inputGraph)
        {
            for (int i = 0; i < _Individuals.Capacity; ++i)
                _Individuals.Add(GenerateIndividual(inputGraph));
        }

        /// <summary>
        /// Generates a random path individual based on specified graph.
        /// </summary>
        /// <param name="inputGraph">The graph to generate random path individual from.</param>
        /// <returns></returns>
        private PathGraph GenerateIndividual(Graph inputGraph)
        {
            // Create 1 to destination - 1 circuit, and shuffle it.
            List<int> MidPathSequence = Enumerable.Range(1, inputGraph.Size - 1).ToList();
            List<int> FullPathSequence = MidPathSequence;
            FullPathSequence.Insert(0, 0);
            FullPathSequence.Add(0);
            PathGraph PathIndividual = new PathGraph(FullPathSequence.Count, inputGraph);
            PathIndividual.SetPath(FullPathSequence.ToArray());
            PathIndividual.Shuffle();
            return PathIndividual;
        }

        #region Interface implementations

        /// <summary>
        /// Gets the number of path individuals in this <see cref="Population"/>.
        /// </summary>
        public int Count => _Individuals.Count;

        public bool IsReadOnly => ((ICollection<PathGraph>)_Individuals).IsReadOnly;

        /// <summary>
        /// Gets or sets the path at specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <returns></returns>
        public PathGraph this[int index]
        {
            get => _Individuals[index];
            set => _Individuals[index] = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() => _Individuals.GetEnumerator();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(PathGraph item) => _Individuals.IndexOf(item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        public void Insert(int index, PathGraph item) => _Individuals.Insert(index, item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index) => _Individuals.RemoveAt(index);
        
        /// <summary>
        /// Adds a path individual to the <see cref="Population"/>.
        /// </summary>
        /// <param name="value">The path individual to be added to the <see cref="Population"/>.</param>
        /// <returns></returns>
        public void Add(PathGraph item) => _Individuals.Add(item);

        /// <summary>
        /// 
        /// </summary>
        public void Clear() => _Individuals.Clear();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(PathGraph item) => _Individuals.Contains(item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(PathGraph[] array, int arrayIndex) => _Individuals.CopyTo(array, arrayIndex);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(PathGraph item) => _Individuals.Remove(item);

        IEnumerator<PathGraph> IEnumerable<PathGraph>.GetEnumerator() => _Individuals.GetEnumerator();

        #endregion
    }
}
