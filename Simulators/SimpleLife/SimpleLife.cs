using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Simulator;
using System.Drawing;

namespace SimpleLife
{
    public class SimpleLife : Simulator.Simulator
    {
        int gridSize;
        int[,] grid;
        public List<Point> refStartState;
        int lastChangeCount;

        public SimpleLife(List<Point> startState)
        {
            gridSize = 41;
            stepLimit = 20000;
            refStartState = startState == null ? null : startState.Select(item => new Point(item.X, item.Y)).ToList();
        }
        override public bool Init(string input)
        {
            inputParseError = false;
            Reset();

            grid = new int[gridSize, gridSize];
            for (int i = 0; i < gridSize; i++)
                for (int j = 0; j < gridSize; j++)
                    grid[i, j] = 0;
            foreach (Point p in refStartState)
                grid[GetCoord(p.X), GetCoord(p.Y)] = 1;

            DrawState();
            lastChangeCount = -1;
            return inputParseError;
        }
        override public void Reset()
        {
            currentTime = 0;
        }
        override public bool Done()
        {
            return lastChangeCount == 0;   // run to count...
        }
        override public void AdvanceState()
        {
            List<Point> flipPoints = new List<Point>();

            /// for each interesting point in the space (start with all)
            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                {
                    int curState = grid[x, y];
                    ///  apply the rules - note if the point would flip state
                    int areaOnCount = GetSurroundingLive(x, y);
                    if ((curState == 0 && areaOnCount == 3) ||
                        (curState == 1 && !(areaOnCount == 2 || areaOnCount == 3)))
                        flipPoints.Add(new Point(x, y));
                }
            currentTime++;
            lastChangeCount = flipPoints.Count;
            /// for each flipped point
            foreach (Point p in flipPoints)
            {
                ///  update the actual state
                grid[p.X, p.Y] = 1 - grid[p.X, p.Y];
            }
            /// show the results
            DrawState();
            
            /// lag ??
            Thread.Sleep(150);
        }

        private int GetCoord(int relVal)
        {
            return gridSize / 2 + relVal;
        }
        private int GetCenteredCoord(int relVal)
        {
            return relVal - gridSize / 2;
        }

        private void DrawState()
        {
            System.Console.SetCursorPosition(0, 0);
            System.Console.WriteLine(currentTime.ToString() + "--------------");
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    System.Console.Write(grid[x, y] == 0 ? " " : "O");
                }
                System.Console.WriteLine("|");
            }
            System.Console.WriteLine(currentTime.ToString() + "--------------");
        }

        private int GetSurroundingLive(int xIn, int yIn)
        {
            int outCount = 0;
            for (int x = xIn - 1; x <= xIn + 1; x++)
                for (int y = yIn - 1; y <= yIn + 1; y++)
                    if (!(x == xIn && y == yIn) && grid[(x + gridSize) % gridSize, (y + gridSize) % gridSize] == 1)
                        outCount++;
            return outCount;
        }

        public List<Point> AddOscillator(int xBase, int yBase)
        {
            List<Point> outList = new List<Point>();
            outList.Add( new Point( xBase-1, yBase+0 ) );
            outList.Add( new Point( xBase+0, yBase+0 ) );
            outList.Add( new Point( xBase+1, yBase+0 ) );
            return outList;
        }
        public List<Point> AddBeacon(int xBase, int yBase)
                    {
            List<Point> outList = new List<Point>();
            outList.Add( new Point( xBase+1, yBase-2 ) );
            outList.Add( new Point( xBase+2, yBase-2 ) );
            outList.Add( new Point( xBase+2, yBase-1 ) );
            outList.Add( new Point( xBase-1, yBase+0 ) );
            outList.Add( new Point( xBase-1, yBase+1 ) );
            outList.Add( new Point( xBase+0, yBase+1 ) );
            return outList;
        }
        public List<Point> AddGlider(int xBase, int yBase)
        {
            List<Point> outList = new List<Point>();
            outList.Add( new Point( xBase-1, yBase-1 ) );
            outList.Add( new Point( xBase+0, yBase+0 ) );
            outList.Add( new Point( xBase+1, yBase-1 ) );
            outList.Add( new Point( xBase+0, yBase+1 ) );
            outList.Add( new Point( xBase+1, yBase+0 ) );
            return outList;
        }
        public List<Point> AddRandom(int pctLive)
        {
            List<Point> outList = new List<Point>();

            for (int x = 0; x < gridSize; x++)
                for (int y = 0; y < gridSize; y++)
                    if (oracle.Next(100) <= pctLive)
                        outList.Add(new Point(GetCenteredCoord(x), GetCenteredCoord(y)));
            return outList;
        }
    }
}
