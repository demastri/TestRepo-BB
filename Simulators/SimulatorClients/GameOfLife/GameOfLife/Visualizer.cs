using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    abstract public class Visualization
    {
        protected Life parent;
        public int displayLagMS = 200;
        public Visualization(Life me)
        {
            parent = me;
        }
        abstract public void DrawState();
    }
}
