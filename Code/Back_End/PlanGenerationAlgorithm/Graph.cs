using System;
using System.Collections.Generic;
using System.Text;

namespace PlanGenerationAlgorithm
{
    class Graph
    {
        public Node Root;
        public List<Node> AllNodes = new List<Node>();

        public Node CreateRoot(string name)
        {
            Root = CreateNode(name);
            return Root;
        }

        public Node CreateNode(string name)
        {
            var n = new Node(name);
            AllNodes.Add(n);
            return n;
        }
    }
}
