using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMADSim
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputStrings = { "10000 0.0", "10000 0.5", "10000 1.0" };

            if (args.Count() == 2)
            {
                inputStrings[0] = args[0] + " " + args[1];
                inputStrings[1] = inputStrings[2] = null;
            }



            foreach (string inputString in inputStrings)
            {
                if (inputString == null)
                    continue;
                string[] s = inputString.Split(' ');
                int count = Convert.ToInt32(s[0]);
                string wPct = s[1];

                Simulator.Simulator mySim = new ThisSim();

                mySim.Init(wPct);
                if (mySim.inputParseError)
                    Console.WriteLine("One Or More Problems Parsing Input File.  Exiting.");
                else
                {
                    double result = mySim.RunList(count);
                    Console.WriteLine("Result for p="+inputString+": w=" + result.ToString("F2") );
                }
            }

        }
    }
}
