using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace GameOfLife
{
    class Graphic2DVisualizer : Visualization
    {
        Visualizer2DForm form;

        Thread t;
        public Graphic2DVisualizer()
            : base()
        {
            form = new Visualizer2DForm();
        }
        public Graphic2DVisualizer(Life p)
            : base(p)
        {
            form = new Visualizer2DForm();
        }

        override public void Open()
        {
            form.game = parent;
            t = new Thread(graphicThreadStartProc);
            t.Start(form);
        }
        override public void Close()
        {
            form.Close();
        }

        static void graphicThreadStartProc(object state)
        {
            Application.Run((Form)state);
        }


        override public void DrawState()
        {
            form.canvas.Invalidate();
            Thread.Sleep(displayLagMS);
        }
    }
}
