using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    internal class Default_1D_Visualizer : Visualization
    {
        public Default_1D_Visualizer(Life p)
            : base(p)
        {
        }

        override public void DrawState()
        {
            // by default, we can say +- 100 centered at 0, with a 3x3 pixel range (vertical dimension is time...)
            // for a  1 d display, this will roll in a drawing space centered on some dialog output space...
            System.Drawing.Point xRange = parent.GetRange(0);

            // for each node in the space
                // find it's location on the canvas
                // draw an appropriate box at that space
        }
    }
}
