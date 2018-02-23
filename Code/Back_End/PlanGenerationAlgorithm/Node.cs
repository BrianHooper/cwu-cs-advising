using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;

namespace PlanGenerationAlgorithm
{
    public class Arc
    {
        public int weight;
        public Node Parent;
        public Node Child;
    }
    public class Node
    {
        public Course Name;
        public List<Arc> Arcs = new List<Arc>();
        public int profit;
        public int weight;
        public int level;
        public Node(Course name)
        {
            Name = name;
        }

        /// <summary>
        /// Create a new arc, connecting this Node to the Nod passed in the parameter
        /// Also, it creates the inversed node in the passed node
        /// </summary>
        public Node AddArc(Node child, int w)
        {
            Arcs.Add(new Arc
            {
                Parent = this,
                Child = child,
                weight = w
            });

            if (!child.Arcs.Exists(a => a.Parent == child && a.Child == this))
            {
                child.AddArc(this, w);
            }

            return this;
        }
    }
}
