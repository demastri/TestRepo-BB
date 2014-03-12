using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject.Specs
{
    class SpecFlowCalculator
    {
        public class InsufficientOperands : Exception
        {
            public InsufficientOperands ()
            {
            }

            public InsufficientOperands (string message)
                : base(message)
            {
            }

            public InsufficientOperands(string message, Exception inner)
                : base(message, inner)
            {
            }
        }

        private List<int> operands;

        private int Pop()
        {
            int i = Peek();
            operands.RemoveAt(0);
            return i;
        }
        public int Peek()
        {
            return operands[0];
        }

        public SpecFlowCalculator()
        {
            operands = new List<int>();
        }

        public void EnterOperand(int i)
        {
            operands.Add(i);
        }

        public void TakeAction(string oper)
        {
            switch (oper)
            {
                case "Add":
                    if (operands.Count < 2)
                        throw new InsufficientOperands(oper + " Exepects 2 operands");
                    else
                        EnterOperand(Pop() + Pop());
                    break;
                default:
                    break;
            }
        }
    }
}
