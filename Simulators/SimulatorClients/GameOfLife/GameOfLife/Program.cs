using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    class Program
    {
        static void Main(string[] args)
        {
            Simulator.Simulator mySim;
            Graphic2DVisualizer graphicCanvas = new Graphic2DVisualizer();

            //mySim = new GameOfLife.Life(2, 4, 2, "B3/S23", 1000, graphicCanvas);             // 2D, 2 states, square grid, Conway's original rules
            mySim = new GameOfLife.Life(2, 3, 2, "B1/S2", 1000, graphicCanvas);             // 2D, 2 states, triangle, Conway's original rules
            //mySim = new GameOfLife.Life(2, -4, 2, "B1/S2", 1000, graphicCanvas);             // 2D, 2 states, square/cross, interesting result for set 'xxx' on...
            //mySim = new GameOfLife.Life(1, 2, 2, "000:0 001:1 010:0 011:1 100:1 101:0 110:1 111:0", 1000000, null); // Wolfram's rule 90

            string inputString;

            //inputString = "[100,40:t] 25,25:1 25,26:1 25,27:1";              // set three explicit points to "on"
            //inputString = "[100,40:f] {75} 50,20:1 49,20:1 51,20:1";              // set three explicit points to "on"   interesting triangle patterns
            inputString = "[100,40:t] {75} Random20";              // 50x50 grid, wrapped, 75 ms delay, start with 20% "on" at random
            //inputString = "[100,30:t] {75} Random05";               // 100x30 grid, wrapped, 75 ms delay, start with 20% "on" at random
            //inputString = "[40,40:t] 30,30:1 31,30:1 32,30:1 32,29:1 31,28:1";    // glider - test grid size
            //inputString = "[10,10:t] 2,2:1 3,2:1 4,2:1 4,1:1 3,0:1";              // glider around corners (loops at 40)
            //inputString = "[20,20:f] 10,10:1 11,10:1 12,10:1 12,09:1 11,08:1";    // glider off the edge

            mySim.trackStates = false;

            if (mySim.inputParseError)
                Console.WriteLine("One Or More Problems Parsing Input File.  Exiting.");
            else
            {
                bool done = false;
                while (!done)
                {
                    mySim.Init(inputString);
                    mySim.breakKeys = new List<char>();
                    mySim.breakKeys.Add('x');
                    mySim.breakKeys.Add('r');
                    mySim.Run();
                    if (mySim.breakKey == 'x')
                        done = true;
                }
                Console.WriteLine("Finished at: t=" + mySim.currentTime.ToString("F0"));
            }
        }
    }
}
