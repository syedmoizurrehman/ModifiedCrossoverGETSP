using System.Collections.Generic;
using System.Linq;

namespace DataStructures
{
    public partial class Graph
    {
        public class Vertex
        {
            public struct Edge
            {
                public int Weight { get; }
                public int Index { get; }

                public Edge(int index, int weight)
                {
                    Weight = weight;
                    Index = index;
                }

                /// <summary>
                /// Override for <see cref="object.ToString()"/>. Displays <see cref="Index"/> of this edge.
                /// </summary>
                public override string ToString() => "Index = " + Index;

                /// <summary>
                /// Override for == operator for value comparison.
                /// </summary>
                public static bool operator ==(Edge obj1, Edge obj2) => obj1.Index == obj2.Index && obj1.Weight == obj2.Weight;

                /// <summary>
                /// Override for != operator for value comparison.
                /// </summary>
                public static bool operator !=(Edge obj1, Edge obj2) => obj1.Index != obj2.Index || obj1.Weight != obj2.Weight;
            }

            public LinkedList<Edge> Edges;
            public int Degree { get => Edges.Count; }
            public int Index { get; }

            public Vertex(int index)
            {
                Index = index;
                Edges = new LinkedList<Edge>();
            }

            public void ConnectTo(int targetIndex, int weight)
            {
                if (weight != 0 && !IsNeighbor(targetIndex))
                    Edges.AddLast(new Edge(targetIndex, weight));
            }

            public void DisconnectFrom(int targetIndex)
            {
                Edge TargetEdge = Edges.FirstOrDefault(X => X.Index == targetIndex);
                if (TargetEdge == new Edge())
                    return; // Default value is returned, therefore the TargetEdge does not exist.
                Edges.Remove(TargetEdge);
            }

            public Edge GetEdge(int targetIndex) => Edges.FirstOrDefault(X => X.Index == targetIndex);

            /// <summary>
            /// O(n) operation.
            /// </summary>
            /// <param name="targetIndex"></param>
            /// <returns></returns>
            public bool IsNeighbor(int targetIndex)
            {
                return Edges.FirstOrDefault(X => X.Index == targetIndex) != new Edge();
            }

            /// <summary>
            /// Gets a value indicating whether this vertex is connected to the vertices specified in parameter indices.
            /// O(n) operation.
            /// </summary>
            /// <param name="predecessorIndex"></param>
            /// <param name="successorIndex"></param>
            /// <returns></returns>
            public bool IsBetween(int predecessorIndex, int successorIndex)
            {
                if (predecessorIndex == successorIndex)
                    return false;

                bool IsPredecessorValid = false;
                bool IsSuccessorValid = false;

                foreach (var _Edge in Edges)
                {
                    if (_Edge.Index == predecessorIndex)
                        IsPredecessorValid = true;

                    else if (_Edge.Index == successorIndex)
                        IsSuccessorValid = true;
                }

                return IsPredecessorValid && IsSuccessorValid;
            }

            public override string ToString() => "Index: " + Index.ToString();
        }
    }
}
