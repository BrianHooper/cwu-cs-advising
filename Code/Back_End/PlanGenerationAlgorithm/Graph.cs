using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    class Graph
    {
        public Node Root;
        public List<Node> AllNodes = new List<Node>();

        public Node CreateRoot(Course name)
        {
            Root = CreateNode(name);
            return Root;
        }

        public Node CreateNode(Course name)
        {
            var n = new Node(name);
            AllNodes.Add(n);
            return n;
        }
    }
}
