using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator;
using System.Drawing;
/// todo
///  x - fix the adh and BS rule logic to make the transition generator more straightforward (don't reject 0,0)
///  consume explicit xin:thisstate:sout
///  add don't care to the transition generator 
///  allow for growing game without bound (even if the visualizer is bound...)
///  x - fix the "cross" geometry
///  x - do a better 2D graphic visualizer
///  do a 1D game

namespace GameOfLife
{
    public partial class Life : Simulator.Simulator
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
            d.parent = this;
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
            if (gStart >= 0 && gEnd > 0 && gStart < gEnd)
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
            canvas.Open();
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
            l = new Location(pos);
            return true;
        }
        override public void Reset()
        {
            Init(refStartState);
        }
        override public bool Done()
        {
            return false; // testing the loop detection code || changedStates == null || changedStates.Keys == null || changedStates.Keys.Count() == 0;
        }
        public override void Cleanup()
        {
            canvas.Close();
        }
        override public void AdvanceState()
        {
            List<string> changed = new List<string>();
            List<Location> universe = FindAllAdjacent(changedStates.Keys.ToList()); // changedStates is implied here...
            changedStates.Clear();
            foreach (Location l in universe)
            {
                int newState = ApplyRules(l);
                if (GetState(l) != newState && OnGrid(l))
                {
                    changedStates[l] = newState;
                }
            }
            currentTime++;
            ApplyChanges();
            canvas.DrawState();
        }

        override public string HashState()
        {
            string outString = "";
            Location l = new Location();
            for (InitUnivPos(ref l); l != null; IncrementUnivPos(ref l))
                outString += GetState(l).ToString();
            return outString;
        }

        private void InitUnivPos(ref Location l)
        {
            for (int d = 0; d < dimensions; d++)
                l.Add(d, GetRange(d).X);
        }
        private void IncrementUnivPos(ref Location l)
        {
            for (int i = dimensions - 1; i >= 0; i--)
            {
                if (l.index[i] == GetRange(i).Y - 1)
                    continue;   // can't increment here...
                l.index[i]++;
                for (int j = i+1; j<dimensions; j++)
                    l.index[j] = GetRange(j).X;
                return;
            }
            l = null;
        }
        // local functions:


        internal int dimensions;
        internal int geometry;
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

}
