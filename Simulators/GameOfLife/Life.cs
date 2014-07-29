using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator;
using System.Drawing;
using JPD.Combinatorics;

namespace GameOfLife
{
    public class Life : Simulator.Simulator
    {
        /// ok, so to call a sim I want to be able to say something like:

        /// Life game = new Life( 1, 2, 2, "000:0 001:1 010:0 011:1 100:1 101:0 110:1 111:0", 1000000, null); // for rule 90
        /// game.Init( "0:1" );

        /// Life game = new Life( 2, 4, 2, "B3/S23", 1000000, null); // for original Conway variant
        /// game.Init( "0,0:1 0,1:1 0,-1:1" );

        /// build steps
        ///  instantiate a 1d game with 2 states
        ///  have it transition through a number of steps cleanly
        ///  visualize this reasonably
        ///
        /// ### need to fix wrap
        /// 
        /// extend for multiple dimensions
        /// extend for multiple states
        /// extend for multiple geometries

        public Life(int dimensionality, int geom, int totStates, string transitions, int limit, Visualization d)
            : base()
        {
            nbrStates = totStates;
            dimensions = dimensionality;    // we'll see, but up to 3 should be easy enough...
            geometry = geom;                // again, we'll see, but 3, 4 and 6 sides should be easy enough to tile through...
            if (d == null && dimensions == 1)
                d = new Default_1D_Visualizer(this);
            if (d == null && dimensions == 2)
                d = new Default_2D_Visualizer(this);
            canvas = d;                     // something to draw into...
            tRules = Transition.BuildFromString(transitions, dimensions, geometry, nbrStates);
            stepLimit = limit;
        }
        override public bool Init(string startState)
        {
            int gStart;
            int gEnd;
            // start state might have displayLagDetail as well
            gStart = startState.IndexOf('{');
            gEnd = startState.IndexOf('}');
            if (gStart >= 0 && gEnd > 0 && gStart < gEnd)
            {
                canvas.displayLagMS = Convert.ToInt32(startState.Substring(gStart + 1, gEnd - gStart - 1));
                startState = startState.Substring(0, gStart) + startState.Substring(gEnd + 1);
            }

            List<int> sizeLimit = new List<int>();
            // start state might have griddimensions as well
            gStart = startState.IndexOf('[');
            gEnd = startState.IndexOf(']');
            if (gStart >=0  && gEnd > 0 && gStart < gEnd )
            {
                string gridStr = startState.Substring(gStart + 1, gEnd - gStart - 1);
                foreach (string s in gridStr.Split(' '))
                {
                    Location loc = null;
                    string stateStr = "";
                    PullIndicesFromString(s, ref loc, ref stateStr);
                    foreach (int i in loc.index)
                        sizeLimit.Add(i);
                    gridWraps = (stateStr == "t");
                }
                startState = startState.Substring(0, gStart) + startState.Substring(gEnd + 1);
            }
            else
            {
                for (int i = 0; i < dimensions; i++)
                    sizeLimit.Add(40);
                gridWraps = true;
            }
            return Init(startState.Trim(), sizeLimit);
        }
        public bool Init(string startState, List<int> gridSize)
        {
            curState = new Dictionary<Location, int>(); // start with an empty grid...
            gridSizeLimit = gridSize;

            refStartState = startState;

            // init ChangedStates here
            changedStates = new Dictionary<Location, int>();
            if (startState.IndexOf("Random") == 0)
            {
                int startCount = Convert.ToInt32(startState.Substring(6));
                for (int i = 0; i < dimensions; i++)
                    startCount *= gridSize[i];
                startCount /= 100;

                for (int i = 0; i <= startCount; i++)
                {
                    List<int> pos = new List<int>();
                    for (int ii = 0; ii < dimensions; ii++)
                        pos.Add(oracle.Next(gridSize[ii]));
                    Location loc = new Location(pos);
                    changedStates[loc] = 1;
                }
            }
            else
            {
                // of the form "0,0:1 0,1:1 0,-1:1"
                foreach (string s in startState.Split(' '))
                {
                    if (s.Trim() == "")
                        continue;
                    Location loc = null;
                    string stateStr = "";
                    PullIndicesFromString(s, ref loc, ref stateStr);
                    int state = Convert.ToInt32(stateStr);
                    changedStates[loc] = state;
                }
            }
            ApplyChanges();
            canvas.DrawState();

            return true;
        }
        private bool PullIndicesFromString(string s, ref Location l, ref string st)
        {
            st = s.Substring(s.IndexOf(':') + 1);
            List<int> pos = new List<int>();
            foreach (string ss in s.Split(new char[] { ',', ':' }))
                if (pos.Count < dimensions)
                    pos.Add(Convert.ToInt32(ss));
            l= new Location(pos);
            return true;
        }
        override public void Reset()
        {
            Init(refStartState);
        }
        override public bool Done()
        {
            return changedStates == null || changedStates.Keys == null || changedStates.Keys.Count() == 0;
        }
        override public void AdvanceState()
        {
            List<string> changed = new List<string>();
            List<Location> universe = FindAllAdjacent(changedStates.Keys.ToList()); // changedStates is implied here...
            changedStates.Clear();
            foreach (Location l in universe)
            {
                int newState = ApplyRules(l);
                if (GetState(l) != newState && OnGrid(l) )
                {
                    changedStates[l] = newState;
                }
            }
            currentTime++;
            ApplyChanges();
            canvas.DrawState();
        }


        // local functions:

        protected class Transition
        {
            int startState;
            int comparator;
            public int endState;
            public Transition(string s)
            {
                string[] l = s.Split('-');
                startState = Convert.ToInt32(l[0]);
                comparator = Convert.ToInt32(l[1]);
                endState = Convert.ToInt32(l[2]);
            }
            public static List<Transition> BuildFromString(string s, int dim, int geom, int nbrStates)
            {
                List<Transition> outList = new List<Transition>();

                /// two cases, explicit and aggregate state transitions -> both can be expressed as explicit transitions
                /// 
                /// we want to be able to state something like:
                ///  newState[cell] = f(surroundingState[cell], currentState[cell])
                ///  where the transition is independent of the start state "All 001 -> state 7", we can set the start to default 
                ///  
                // ###

                /// "000:0 001:1 010:0 011:1 100:1 101:0 110:1 111:0"
                ///  this translates into 8 explicit default transitions for a 1D/2 state game centered on the current cell
                /// 
                // ###

                /// "B3/S23"
                ///  this translates into 1024 explicit default transitions for a 2D/2 state game centered on the current cell
                ///  default is care +-1 in every direction for each dim -> 2D = 9 total cells, 2^9 states = 512 rules/start state
                ///  B implies rules that start in state 0, S implies rules that start in state 1
                ///  THe numbers imply the sum of items in the grid around the cell in a non-zero state 
                ///    (This has to be addressed for higher state games)
                if (s.IndexOf("B") == 0)
                {
                    List<int> born = new List<int>();
                    for (int i = 1; i < s.IndexOf("/"); i++)
                        born.Add(s[i]-'0');
                    List<int> survive = new List<int>();
                    for (int i = s.IndexOf("S") + 1; i < s.Length; i++)
                        survive.Add(s[i]-'0');
                    List<int> org = new List<int>();
                    for (int i = 0; i < dim; i++)
                        org.Add(0);
                    Location origin = new Location(org);
                    int states = (int)System.Math.Pow(nbrStates, origin.FindAdjacent(geom, false, new List<int>()).Count);
                    for (int i = 0; i < states; i++)
                    {
                        string adjStr = getBinaryString(i);
                        int adjSum = getBinaryCount(i);
                        // do B transition
                        outList.Add(new Transition("0-" + adjStr + (born.Contains(adjSum) ? "-1" : "-0")));
                        // do S transition
                        outList.Add(new Transition("1-" + adjStr + (survive.Contains(adjSum) ? "-1" : "-0")));
                    }
                }

                return outList;
            }

            private static string getBinaryString(int i)
            {
                string outString = "";
                while (i > 0)
                {
                    outString = (i % 2 == 1 ? "1" : "0") + outString;
                    i /= 2;
                }
                return outString == "" ? "0" : outString;
            }
            private static int getBinaryCount(int i)
            {
                int outCount = 0;
                while( i > 0 )
                {
                    if( i % 2 == 1 )
                        outCount++;
                    i /= 2;
                }
                return outCount;
            }
            public static Transition Find(List<Transition> l, int start, int context)
            {
                foreach (Transition t in l)
                    if (t.startState == start && t.comparator == context)
                        return t;
                return null;
            }

        }

        internal int dimensions;
        protected int geometry;
        protected bool gridWraps;
        private Visualization canvas;
        private List<Transition> tRules;
        private string refStartState;
        internal List<int> gridSizeLimit;    // MAXINT = unbounded...
        protected int nbrStates;
        
        internal Dictionary<Location, int> curState;
        private Dictionary<Location, int> changedStates;

        internal Point GetRange(int dim)
        {
            return new Point(0, gridSizeLimit[dim]);
        }

        private List<Location> FindAllAdjacent(List<Location> thisState)
        {
            List<Location> outSet = new List<Location>();
            foreach (Location s in thisState)
            {
                List<Location> these = s.FindAdjacent(geometry, gridWraps, gridSizeLimit);
                foreach (Location ss in these)
                {
                    if (!outSet.Contains(ss))
                    {
                        outSet.Add(ss);
                    }
                }
            }
            return outSet;
        }
        internal string FindAdjacentString(Location thisLoc)
        {
            string outString = "";
            List<Location> outLoc = thisLoc.FindAdjacent(geometry, gridWraps, gridSizeLimit);
            foreach (Location l in outLoc)
                outString += GetState(l).ToString();
            return outString;
        }

        private int ApplyRules(Location thisState)
        {
            int initState = GetState(thisState);
            string stateStr = FindAdjacentString(thisState);
            Transition t = Transition.Find(tRules, initState, Convert.ToInt32(stateStr));
            if (t != null)
                return t.endState;
            return initState;
        }
        private void ApplyChanges()
        {
            foreach (Location l in changedStates.Keys)
            {
                curState[l] = changedStates[l];
            }
        }
        private int GetState(Location l)
        {
            return !curState.Keys.Contains(l) ? 0 : curState[l];
        }
        private bool OnGrid(Location l)
        {
            for (int i = 0; i < dimensions; i++)
                if (l.index[i] < GetRange(i).X || l.index[i] >= GetRange(i).Y)
                    return false;
            return true;
        }

    }

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
        static List<Location> rectExcl = new List<Location>() { new Location("0,0") };
        static List<int> triAdj = new List<int>() { -1, 0, 1 };
        static List<Location> triExcl = new List<Location>() { new Location("-1,-1"), new Location("-1,1"), new Location("1,-1"), new Location("1,1") };
        static List<int> hexAdj = new List<int>() { -1, 0, 1 };

        internal List<Location> FindAdjacent(int geom, bool wrap, List<int> gridSize)
        {
            if (geom == 4) // rectangular
            {
                return FindAdjacent(rectAdj, rectExcl, wrap, gridSize );
            }
            if (geom == 6) // hexagonal
            {
                return FindAdjacent(rectAdj, null, wrap, gridSize);
            }
            if (geom == 3) // triangle tesselation
            {
                return FindAdjacent(triAdj, triExcl, wrap, gridSize);
            }
            return null; // didn't know anything about this geometry...
        }
        internal List<Location> FindAdjacent(List<int> allowedDelta, List<Location> excludedComb, bool wrap, List<int> gridSize)
        {
            // for every dimension, we must allow for -1, 0, +1 states
            // this is really a variations / repitition problem, for the mapping (-1,0,-1) select "dimensions" of them
            List<Location> outList = new List<Location>();
            JPD.Combinatorics.Variations<int> var = new Variations<int>((uint)index.Count, 3, true, allowedDelta);

            for (var.First(); !var.AtEnd; var.Next())
            {
                if (excludedComb == null || !excludedComb.Contains(new Location(var.Current.ToList())))
                {
                    Location l = new Location();
                    for (int d = 0; d < index.Count; d++)
                    {
                        int target = index[d] + var.Current[d];
                        if ( wrap )
                            target = (target +gridSize[d]) % gridSize[d];
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
