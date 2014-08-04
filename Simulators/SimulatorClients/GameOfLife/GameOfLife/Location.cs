using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPD.Combinatorics;

namespace GameOfLife
{
    public class Location   // abstraction for a position address
    {
        /// a position is simply an n-dimensional array of index offsets
        /// for a "rectangular" grid in n-dim, there are 3^n neighbors (including the base position), corresponding to the 1 0 -1 surrounding locations for each dimension
        /// For the rectangular grid (square for evenly scaled axes), the centers are at each point in the 2D grid, and the "adj" points are -
        ///     -1,-1 -1,0 -1,1    0,-1 0,0 0,1    1,-1 1,0 1,1
        ///     
        /// For the triangular grid, the "centers" are at integer points on the 2D grid for "even" rows, and 1/2 points for "odd" rows
        ///     for completeness, each unit on the vertical scale is sqrt(3)/2 the unit on the horizontal scale
        ///     the adj points are (depending) -1,0 .1,0 and 0,-1 for "odd" rows or 0,1 for "even" rows
        ///     the actual centers are slightly offset in the y-dim for adjacent elements because the "centers" bias towards the base
        ///     as a perfect analog for the rectangular case, you can treat it as the same 9 neighbors in 2D

        /// as a practical matter, for "standard geometries" there are fixed transitions embedded in this class
        /// for non-standard geometries, there are lists that detemrine what the transitions are ### until we encounter them
        /// dim     geom        transitions     visualizer
        /// 2       3           yes             ###
        /// 2       4           yes             ###
        /// 2       6           ###             ###
        /// 3       3           ###             ###
        /// 3       3           ###             ###

        public List<int> index;    // MAXINT is undefined...
        public Location()
        {
            index = new List<int>();
        }
        public Location(string s)
        {
            index = new List<int>();
            foreach (string ss in s.Split(','))
            {
                index.Add(Convert.ToInt32(ss));
            }
        }
        public Location(List<int> s)
        {
            index = s.Select(item => item).ToList();
        }
        public void Add(int dim, int val)
        {
            while (index.Count <= dim)
                index.Add(Int32.MaxValue);
            index[dim] = val;
        }
        public void Set(int dim, int val)
        {
            Add(dim, val);
        }

        static List<int> rectAdj = new List<int>() { -1, 0, 1 };
        static List<Location> rectExcl = new List<Location>();
        static List<int> triAdj = new List<int>() { -1, 0, 1 };
        static List<Location> triExcl = new List<Location>() { new Location("-1,-1"), new Location("-1,1"), new Location("1,-1"), new Location("1,1") };
        static List<Location> crossExcl = triExcl;
        static List<Location> triOddExcl = new List<Location>() { new Location("0,1") };
        static List<Location> triEvenExcl = new List<Location>() { new Location("0,-1") };
        static List<int> hexAdj = new List<int>() { -1, 0, 1 };

        internal List<Location> FindAdjacent(int geom, bool wrap, List<int> gridSize)
        {
            if (geom == 4) // rectangular
            {
                return FindAdjacent(rectAdj, rectExcl, null, null, wrap, gridSize);
            }
            if (geom == -4) // cross
            {
                return FindAdjacent(rectAdj, crossExcl, null, null, wrap, gridSize);
            }
            if (geom == 6) // hexagonal
            {
                return FindAdjacent(rectAdj, null, null, null, wrap, gridSize);
            }
            if (geom == 3) // triangle tesselation
            {
                return FindAdjacent(triAdj, triExcl, triOddExcl, triEvenExcl, wrap, gridSize);
            }
            return null; // didn't know anything about this geometry...
        }
        internal List<Location> FindAdjacent(List<int> allowedDelta, List<Location> excludedComb, List<Location> excludedOddRowComb, List<Location> excludedEvenRowComb, bool wrap, List<int> gridSize)
        {
            // for every dimension, we must allow for -1, 0, +1 states
            // this is really a variations / repitition problem, for the mapping (-1,0,-1) select "dimensions" of them
            List<Location> outList = new List<Location>();
            JPD.Combinatorics.Variations<int> var = new Variations<int>((uint)index.Count, 3, true, allowedDelta);

            for (var.First(); !var.AtEnd; var.Next())
            {
                bool exclude = (excludedComb != null && excludedComb.Contains(new Location(var.Current.ToList())))
                    || ((index[0] % 2 == 1) && excludedOddRowComb != null && excludedOddRowComb.Contains(new Location(var.Current.ToList())))
                    || ((index[0] % 2 == 0) && excludedEvenRowComb != null && excludedEvenRowComb.Contains(new Location(var.Current.ToList())));

                if (!exclude)
                {
                    Location l = new Location();
                    for (int d = 0; d < index.Count; d++)
                    {
                        int target = index[d] + var.Current[d];
                        if (wrap)
                            target = (target + gridSize[d]) % gridSize[d];
                        l.Add(d, target);
                    }
                    outList.Add(l);
                }
            }
            return outList;
        }
        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;
            Location thisLoc = obj as Location;
            if ((System.Object)thisLoc == null || (index.Count != thisLoc.index.Count))
                return false;
            for (int i = 0; i < index.Count; i++)
                if (index[i] != thisLoc.index[i])
                    return false;
            return true;
        }
        public override int GetHashCode()
        {
            int outVal = 0;
            for (int i = 0; i < index.Count; i++)
                outVal = outVal ^ index[i];
            return outVal;
        }
    }
}
