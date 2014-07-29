using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SimpleLife
{
    class Program
    {
        static void Main(string[] args)
        {
            SimpleLife mySim = new SimpleLife(null);
            mySim.stepLimit = 500;


            while( true ) {
                //mySim.refStartState = mySim.AddBeacon( 1, 0 );
                //mySim.refStartState = mySim.AddOscillator( 1, 0 );
                //mySim.refStartState = mySim.AddGlider( 1, 0 );
                mySim.refStartState = mySim.AddRandom(60);

                mySim.Init("");
                mySim.Run();
            }
        }
    }
}
