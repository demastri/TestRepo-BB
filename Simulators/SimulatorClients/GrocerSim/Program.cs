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
            string inputString = (args.Count() < 1 ? "../../inputs/input 1.txt" : args[0]);
            
            Simulator.Simulator mySim = new ThisSim();
            
            mySim.Init(inputString);

            if (mySim.inputParseError)
                Console.WriteLine("One Or More Problems Parsing Input File.  Exiting.");
            else
            {
                mySim.Run();
                Console.WriteLine("Finished at: t=" + mySim.result.ToString("F0") + " minutes");
            }
        }
    }
}
