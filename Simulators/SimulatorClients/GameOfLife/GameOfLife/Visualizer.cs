using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameOfLife
{
    abstract public class Visualization
    {
        public Life parent;
        public int displayLagMS = 200;

        public Visualization()
        {
            parent = null;
        }
        public Visualization(Life me)
        {
            parent = me;
        }

        abstract public void DrawState();

        virtual public void Open()
        {
            // if I write to the console...nothing to do here...
        }
        virtual public void Close()
        {
            // if I write to the console...nothing to do here...
        }
    }
}
