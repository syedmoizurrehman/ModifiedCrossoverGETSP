using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public partial class Graph
    {
        public Vertex[] Vertices { get; private set; }

        public int Size { get; private set; }

        public Graph(int vertices)
        {
            Size = vertices;
            Vertices = new Vertex[Size];

            for (int i = 0; i < Size; ++i)
                Vertices[i] = new Vertex(i);
        }

        public void InsertEdge(int sourceIndex, int targetIndex, int weight)
        {
            Vertices[sourceIndex].ConnectTo(targetIndex, weight);
            Vertices[targetIndex].ConnectTo(sourceIndex, weight);
        }

        public void DeleteEdge(int sourceIndex, int targetIndex)
        {
            Vertices[sourceIndex].DisconnectFrom(targetIndex);
            Vertices[targetIndex].DisconnectFrom(sourceIndex);
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
                    Matrix.Append(AdjMatrix[i, j].ToString().PadLeft(3, ' ') + " ");
                }
                Matrix.Append('\n');
            }

            return Matrix.ToString();
        }

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
    }
}
