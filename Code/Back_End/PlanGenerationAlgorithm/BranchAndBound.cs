//import statements
using System;
using System.Collections.Generic;
using System.Text;
using Database_Object_Classes;
using System.Linq;
using Db4objects.Db4o.Internal.Btree;
using System.Xml.Linq;

namespace PlanGenerationAlgorithm
{

    class BranchAndBound
    {
        List<Course> path;
        Heap<Course> right;
        Queue<Node> Q;
        private int Value { get; }
        private int Weight { get; }
        private bool IsChosen { get; set; }
        Stack<Course> left = new Stack<Course>();
        public BranchAndBound(int value, int weight, bool isChosen)
        {
            Value = value;
            Weight = weight;
            IsChosen = isChosen;
        }
        public int GetValue()
        {
            return Value;
        }
        public int GetWeight()
        {
            return Weight;
        }
        public bool GetIsChosen()
        {
            return IsChosen;
        }
        public void GetIsChosen(bool isChosen)
        {
            IsChosen = isChosen;
        }
        int bound(Node u, int n, int W, Course [] arr)
        {
            // if weight overcomes the knapsack capacity, return
            // 0 as expected bound
            if (u.weight >= W)
                return 0;

            // initialize bound on profit by current profit
            int profit_bound = u.profit;

            // start including items from index 1 more to current
            // item index
            int j = u.level + 1;
            int totweight = u.weight;

            // checking index condition and knapsack capacity
            // condition
            while ((j < n) && (totweight + arr[j].weight <= W))
            {
                totweight += arr[j].weight;
                profit_bound += arr[j].value;
                j++;
            }

            // If k is not n, include last item partially for
            // upper bound on profit
            if (j < n)
                profit_bound += (W - totweight) * arr[j].value /
                                                 arr[j].weight;

            return profit_bound;
        }
        bool cmp(Course c,Course c2)
        {
            double r1 = (double)c.value / c.weight;
            double r2 = (double)c2.value / c2.weight;
            return r1 > r2;
        }
        public bool CheckVisited(Course toCheck, List<Course> visited)
        {
            foreach (Course c in visited)
            {
                if (c == toCheck)
                    return true;
            }
            return false;
        }
        List<Course> FindChosenItems(List<Course> list)
        {
            return list;
        }
    }
    }
