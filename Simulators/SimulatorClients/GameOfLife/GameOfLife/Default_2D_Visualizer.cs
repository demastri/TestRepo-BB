using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameOfLife
{
    internal class Default_2D_Visualizer : Visualization
    {
        public Default_2D_Visualizer(Life p)
            : base(p)
        {
        }

        override public void DrawState()
        {
            // by default, we can say +- 100 fixed grid centered at 0,0 , with a 3x3 pixel range 
            // for a  1 d display, this will roll in a drawing space centered on some dialog output space...
            System.Drawing.Point xRange = parent.GetRange(0);
            System.Drawing.Point yRange = parent.GetRange(1);

            // for each node in the space
            // find it's location on the canvas
            // draw an appropriate box at that space

            System.Console.SetCursorPosition(0, 0);
            System.Console.WriteLine(parent.currentTime.ToString() + "--------------");
            Location l = new Location(new List<int>(new int[] { Int32.MaxValue, Int32.MaxValue }));
            for (int y = 0; y < parent.gridSizeLimit[1]; y++)
            {
                l.Set(1, y);
                for (int x = 0; x < parent.gridSizeLimit[0]; x++)
                {
                    l.Set(0, x);
                    System.Console.Write((parent.curState.Keys.Contains(l) && parent.curState[l] == 1) ? "O" : " ");
                }
                System.Console.WriteLine("|");
            }
            System.Console.WriteLine(parent.currentTime.ToString() + "--------------");
            if (parent.inALoop)
                System.Console.WriteLine("--- Loop ("+parent.loopLength.ToString()+") found");
            else
                System.Console.WriteLine("---                                              ");
            Thread.Sleep(displayLagMS);
        }
    }
}
