using DataStructures;
using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    /// <summary>
    /// This class represents a path graph which is a subset of a <see cref="Graph"/> object and contains a sequence of vertices.
    /// The number of vertices cannont be changed after initialization.
    /// </summary>
    public class PathGraph
    {
        private Graph.Vertex[] _PathVertices;

        /// <summary>
        /// Gets a value indicating whether the <see cref="PathGraph"/> is a circuit.
        /// </summary>
        public bool IsCircuit => _PathVertices.First() == _PathVertices.Last();

        /// <summary>
        /// Gets the graph which is the superset of this path graph.
        /// </summary>
        public Graph SupersetGraph { get; }

        /// <summary>
        /// Gets sum of weights of all edges in the path.
        /// </summary>
        public int Distance => CalculateDistance();

        /// <summary>
        /// Calculates edge distances for whole path.
        /// </summary>
        /// <returns></returns>
        private int CalculateDistance()
        {
            int X = 0;
            for (int i = 0; i < Count - 1; ++i)
                X += SupersetGraph.GetEdgeWeight(_PathVertices[i].Index, _PathVertices[i + 1].Index);

            return X;
        }

        /// <summary>
        /// Gets number of vertices in this path.
        /// </summary>
        public int Count => _PathVertices.Length;

        /// <summary>
        /// Gets or sets the vertex at the specified index in the path.
        /// </summary>
        /// <param name="index">The zero-based index of the vertex in path to get or set.</param>
        public Graph.Vertex this[int index] { get => _PathVertices[index]; set => _PathVertices[index] = value; }

        /// <summary>
        /// Initializes a new subset of specified <see cref="Graph"/> which is a path graph described by specified sequence of indices.
        /// </summary>
        /// <param name="graph">Array of indices in order of edge connection.</param>
        /// <param name="size"></param>
        public PathGraph(int size, Graph graph)
        {
            SupersetGraph = graph;
            _PathVertices = new Graph.Vertex[size];
        }

        /// <summary>
        /// Sets the sequence of vertices in the path using specified sequence.
        /// </summary>
        /// <param name="indices"></param>
        public void SetPath(int[] indices)
        {
            // Add all vertices to the path except last vertex.
            for (int i = 0; i < indices.Length - 1; ++i)
                _PathVertices[i] = SupersetGraph[indices[i]];

            // Add last vertex.
            _PathVertices[Count - 1] = SupersetGraph[indices.Last()];
        }

        /// <summary>
        /// Shuffles the vertices in the path randomly. Preserve the first and last vertex if path is a circuit.
        /// </summary>
        public void Shuffle()
        {
            if (IsCircuit)
                _PathVertices.ShuffleSubset(1, Count - 2);

            else
                _PathVertices.Shuffle();
        }

        public override string ToString()
        {
            string Result = "";
            foreach (var V in _PathVertices)
            {
                if (V == null)
                    Result += "null, ";

                else
                    Result += V.Index + ", ";
            }
            return Result.Remove(Result.Length - 2);
        }

        /// <summary>
        /// Determines whether this <see cref="PathGraph"/> contains specified vertex by using default equality comparer.
        /// </summary>
        /// <param name="vertex">The vertex to locate in this <see cref="PathGraph"/></param>
        /// <returns></returns>
        public bool Contains(Graph.Vertex vertex) => _PathVertices.Contains(vertex);

        public IEnumerator<Graph.Vertex> GetEnumerator() => ((ICollection<Graph.Vertex>)_PathVertices).GetEnumerator();
    }
}
