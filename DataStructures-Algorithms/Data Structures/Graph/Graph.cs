using Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public partial class Graph : IEnumerable
    {
        public Vertex[] Vertices { get; private set; }

        public bool IsUndirected { get; }

        /// <summary>
        /// Gets total number of vertices in this graph.
        /// </summary>
        public int Size { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Graph"/> with specified number of vertices.
        /// </summary>
        /// <param name="count">The total number of vertices in this <see cref="Graph"/>. This cannot be changed.</param>
        public Graph(int count, bool isUndirected = true)
        {
            IsUndirected = isUndirected;
            Size = count;
            Vertices = new Vertex[Size];

            for (int i = 0; i < Size; ++i)
                Vertices[i] = new Vertex(i);
        }

        public void InsertEdge(int sourceIndex, int targetIndex, int weight)
        {
            Vertices[sourceIndex].ConnectTo(targetIndex, weight);
            if (IsUndirected)
                Vertices[targetIndex].ConnectTo(sourceIndex, weight);
        }

        public void DeleteEdge(int sourceIndex, int targetIndex)
        {
            Vertices[sourceIndex].DisconnectFrom(targetIndex);
            if (IsUndirected)
                Vertices[targetIndex].DisconnectFrom(sourceIndex);
        }

        /// <summary>
        /// Gets weight of the edge from source vertex to destination vertex.
        /// </summary>
        /// <param name="sourceIndex"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public int GetEdgeWeight(int sourceIndex, int targetIndex)
        {
            Vertex.Edge TargetEdge = Vertices[sourceIndex].GetEdge(targetIndex);

            if (TargetEdge == new Vertex.Edge())
                throw new Exception(); // Default value is returned, therefore no edge between sourceIndex and targetIndex exist.

            return TargetEdge.Weight;
        }

        public string ToMatrixString()
        {
            int[,] AdjMatrix = new int[Size, Size];
            StringBuilder Matrix = new StringBuilder();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    AdjMatrix[i, j] = Vertices[i].GetEdge(j).Weight;
                    //if (AdjMatrix[i, j] == 0)
                    //Matrix.Append("-".PadLeft(3, ' ') + " ");
                    //else
                    Matrix.Append(AdjMatrix[i, j].ToString().PadLeft(3, ' ') + " ");
                }
                Matrix.Append('\n');
            }

            return Matrix.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder List = new StringBuilder();
            foreach (var item in Vertices)
            {
                List.Append(item.Index + ": ");
                foreach (var e in item.Edges)
                {
                    List.Append(e.Index.ToString().PadLeft(2, '0') + "," + e.Weight.ToString().PadLeft(3, '0') + " ");
                }
                List.Append("\n");
            }
            return List.ToString();
        }

        public bool IsTriangleInequalitySatisfied()
        {
            if (!IsUndirected)
                return false;

            for (int i = 0; i < Size; ++i)
            {
                for (int j = 0; j < Size; ++j)
                {
                    if (j == i)
                        continue;
                    for (int k = 0; k < Size; ++k)
                    {
                        if (k == i || k == j)
                            continue;

                        if (GetEdgeWeight(i, j) > GetEdgeWeight(i, k) + GetEdgeWeight(k, j))
                            return false;
                    }
                }
            }
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => Vertices.GetEnumerator();

        /// <summary>
        /// Gets or sets the vertex at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the vertex to get or set.</param>
        /// <returns>A <see cref="Vertex"/> object at the specified index.</returns>
        public Vertex this[int index]
        {
            get => Vertices[index];
            set => Vertices[index] = value;
        }
    }
}