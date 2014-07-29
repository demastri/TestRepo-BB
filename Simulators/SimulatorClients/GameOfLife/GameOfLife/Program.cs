using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrocerSim
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputString = "25,25:1 25,26:1 25,27:1";
            inputString = "[50,50:t] {75} Random20";
            //inputString = "[40,40:t] 30,30:1 31,30:1 32,30:1 32,29:1 31,28:1";    // glider - test grid size
            //inputString = "[20,20:t] 10,10:1 11,10:1 12,10:1 12,09:1 11,08:1";    // glider around corners
            //inputString = "[20,20:f] 10,10:1 11,10:1 12,10:1 12,09:1 11,08:1";    // glider off the edge

            Simulator.Simulator mySim = new GameOfLife.Life(2, 4, 2, "B3/S23", 1000, null); // for rule 90
            //Simulator.Simulator mySim = new GameOfLife.Life(1, 2, 2, "000:0 001:1 010:0 011:1 100:1 101:0 110:1 111:0", 1000000, null); // for rule 90
            
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
