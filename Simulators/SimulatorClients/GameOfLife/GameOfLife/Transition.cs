using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    partial class Life
    {
        protected partial class Transition
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
                /// actually, this means "x:000:0 x:001:1 x:"010:0 x:011:1 x:100:1 x:101:0 x:110:1 x:111:0"
                /// since we don't care what the input state is - (or more accurately, it's embedded...
                /// maybe for explicit transitions, I should not exclude the state, and assume it's embedded!!
                /// also a transition like "00x:0" would mean I don't care about the third one...
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
                        born.Add(s[i] - '0');
                    List<int> survive = new List<int>();
                    for (int i = s.IndexOf("S") + 1; i < s.Length; i++)
                        survive.Add(s[i] - '0');
                    List<int> org = new List<int>();
                    for (int i = 0; i < dim; i++)
                        org.Add(0);
                    Location origin = new Location(org);
                    List<Location> states = origin.FindAdjacent(geom, false, new List<int>());
                    int stateCount = (int)System.Math.Pow(nbrStates, states.Count);
                    // where's waldo?  the 0,0 state shouldn't be included in the sum
                    int waldo = states.IndexOf(origin);
                    for (int i = 0; i < stateCount; i++)
                    {
                        // where's waldo?  the 0,0 state shouldn't be included in the sum
                        string adjStr = getBinaryString(i, states.Count);
                        int adjSum = getBinaryCount(i) - (adjStr.Length > waldo && adjStr[waldo] == '1' ? 1 : 0);
                        // do B transition
                        outList.Add(new Transition("0-" + adjStr + (born.Contains(adjSum) ? "-1" : "-0")));
                        // do S transition
                        outList.Add(new Transition("1-" + adjStr + (survive.Contains(adjSum) ? "-1" : "-0")));
                    }
                }

                return outList;
            }

            private static string getBinaryString(int i, int pad)
            {
                string outString = "";
                while (i > 0)
                {
                    outString = (i % 2 == 1 ? "1" : "0") + outString;
                    i /= 2;
                }
                return outString.PadLeft(pad).Replace(' ', '0');
            }
            private static int getBinaryCount(int i)
            {
                int outCount = 0;
                while (i > 0)
                {
                    if (i % 2 == 1)
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
    }
}
