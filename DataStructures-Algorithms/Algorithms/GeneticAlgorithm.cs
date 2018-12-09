using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DataStructures;
using Extensions;

namespace Algorithms
{
    public enum CrossoverOperator
    {
        Default = 0, CycleCrossover = 1
    }

    public static class GeneticAlgorithm
    {
        /// <summary>
        /// Used by Cycle Crossover for marking vertices in a cycle.
        /// </summary>
        private struct Mark
        {
            public bool IsMarked;
            public int Order;

            public void Set(bool marked, int order) { IsMarked = marked; Order = order; }

            public override string ToString() => $"Marked: {IsMarked}, Order: {Order}";
        }

        /// <summary>
        /// Gets number of generations i.e. number of times CurrentPopulation has evolved.
        /// </summary>
        public static int Generations { get; private set; }

        public static Graph Input { get; set; }

        /// <summary>
        /// Gets or set the number of path individuals in subset population used when selecting individuals for breeding.
        /// </summary>
        public static int TournamentSize { get; set; }

        /// <summary>
        /// Gets or sets the rate at which the genes (vertices) of a path individual are mutated.
        /// </summary>
        public static double MutationRate { get; set; }

        /// <summary>
        /// Gets or sets the rate at which the individuals of the population are bred to produce offspring.
        /// </summary>
        public static double CrossoverRate { get; set; }

        public static Population CurrentPopulation { get; private set; }
        
        /// <summary>
        /// Gets or sets which crossover operator to use.
        /// </summary>
        public static CrossoverOperator CrossoverOperator { get; set; }

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
            CrossoverOperator = CrossoverOperator.Default;
        }

        public static void InitializePopulation(int size)
        {
            Generations = 0;
            CurrentPopulation = new Population(Input, size);
        }

        /// <summary>
        /// Mutates a path indivudual according to <see cref="MutationRate"/>.
        /// </summary>
        /// <param name="pathIndividual">The path individual to mutate.</param>
        public static void Mutate(PathGraph pathIndividual)
        {
            // Iterate through all path vertices
            for (int i = 1; i < pathIndividual.Count - 1; ++i)
            {
                // Use mutation rate
                if (ThreadSafeRandom.CurrentThreadsRandom.NextDouble() < MutationRate)
                {
                    // Get a random index in the path
                    int RandomIndex = ThreadSafeRandom.CurrentThreadsRandom.Next(1, pathIndividual.Count - 2);

                    // Swap the vertices at target position in path
                    var Temp = pathIndividual[i];
                    pathIndividual[i] = pathIndividual[RandomIndex];
                    pathIndividual[RandomIndex] = Temp;
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
            // Use Crossover rate
            if (ThreadSafeRandom.CurrentThreadsRandom.NextDouble() < CrossoverRate)
            {
                switch (CrossoverOperator)
                {
                    case CrossoverOperator.Default:
                        return DefaultCrossover(firstParent, secondParent);

                    case CrossoverOperator.CycleCrossover:
                        PathGraph[] Child = CycleCrossover(firstParent, secondParent);
                        if (Child[0].Distance < Child[1].Distance)
                            return Child[0];

                        return Child[1];

                    default:
                        return null;
                }
            }
            else
            {
                if (firstParent.Distance < secondParent.Distance)
                    return firstParent;

                return secondParent;
            }
        }

        private static PathGraph[] CycleCrossover(PathGraph Parent1, PathGraph Parent2)
        {
            Mark[] P1Marked = new Mark[Parent1.Count];
            Mark[] P2Marked = new Mark[Parent1.Count];
            PathGraph Child1 = new PathGraph(Parent1.Count, Parent1.SupersetGraph);
            PathGraph Child2 = new PathGraph(Parent1.Count, Parent1.SupersetGraph);

            for (int i = 1; i < Parent1.Count - 1; ++i)
            {
                if (P1Marked[Parent1[i].Index].IsMarked == true)
                    continue;

                Graph.Vertex X1 = Parent1[i]; P1Marked[X1.Index].Set(true, i);
                Graph.Vertex X2 = Parent2[i]; P2Marked[X2.Index].Set(true, i);
                do
                {
                    X1 = Parent1[Parent1.IndexOf(X2)]; P1Marked[X1.Index].Set(true, i);
                    X2 = Parent2[Parent1.IndexOf(X2)]; P2Marked[X2.Index].Set(true, i);
                } while (X2 != Parent1[i]);
            }

            // Set initial and final vertices to 0
            Child1[0] = Parent1[0];
            Child2[0] = Parent1[0];
            Child1[Parent1.Count - 1] = Parent1[0];
            Child2[Parent1.Count - 1] = Parent1[0];

            // Fill the children from cycles detected above
            for (int i = 1; i < Parent2.Count - 1; ++i)
            {
                if (P1Marked[Parent1[i].Index].Order % 2 == 0)         // 1, 3, 5 - Cycles
                {
                    Child1[i] = Parent1[i];
                    Child2[i] = Parent2[i];
                }
                else                                            // 2, 4, 6 - Cycles
                {
                    Child1[i] = Parent2[i];
                    Child2[i] = Parent1[i];
                }
            }
            return new PathGraph[] { Child1, Child2 };
        }

        private static PathGraph DefaultCrossover(PathGraph firstParent, PathGraph secondParent)
        {
            int CircuitLength = Input.Size + 1;

            // Create new child path
            PathGraph ChildPath = new PathGraph(CircuitLength, Input);
            ChildPath[0] = Input[0];
            ChildPath[CircuitLength - 1] = Input[0];

            // Get random start and end indices positions for firstParent
            int StartPos = ThreadSafeRandom.CurrentThreadsRandom.Next(1, CircuitLength - 1);
            int EndPos = ThreadSafeRandom.CurrentThreadsRandom.Next(1, CircuitLength - 1) + ThreadSafeRandom.CurrentThreadsRandom.Next(1, CircuitLength - 1);

            // Iterate firstParent to add subset to the child
            for (int i = 1; i < CircuitLength - 1; ++i)
            {
                if (StartPos < EndPos && i > StartPos && i < EndPos)
                    ChildPath[i] = firstParent[i];

                else if (StartPos > EndPos)
                {
                    if (i >= StartPos || i <= EndPos)
                        ChildPath[i] = firstParent[i];
                }
            }

            // Iterate secondParent's vertices to fill the remaining vertices
            for (int i = 1; i < CircuitLength - 1; ++i)
            {
                // Add vertex if child does not contain it already
                if (!ChildPath.Contains(secondParent[i]))
                {
                    // Find an empty position in child
                    for (int j = 1; j < CircuitLength - 1; ++j)
                    {
                        // Add vertex to empty spot
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

        /// <summary>
        /// Selects a candidate path individual from CurrentPopulation for crossover.
        /// </summary>
        /// <returns></returns>
        private static PathGraph TournamentSelect()
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