using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulator
{
    abstract public class Simulator
    {
        static protected Random oracle = new Random();

        public bool executionError { get; set; }
        public bool inputParseError { get; set; }
        public bool stepCountError { get; set; }
        public int currentTime { get; set; }
        public double result{ get; set; }
        public int stepLimit = -1;
        public string inputStr;

        public List<char> breakKeys = null;
        public char breakKey;

        public abstract bool Init(string s);
        public abstract bool Done();
        public abstract void AdvanceState();
        public abstract void Reset();

        public double Run()
        {
            int currentStep = currentTime = 0;

            while (!Done() && (++currentStep < stepLimit || stepLimit == -1))
            {
                if( breakKeys != null && Console.KeyAvailable && breakKeys.Contains(breakKey=Console.ReadKey(false).KeyChar) )
                    break;
                breakKey = '\0';
                AdvanceState();
            }
            return result;
        }
        public double RunList(int count)
        {
            double stdDev=0;
            return RunList(count, ref stdDev);
        }
        public double RunList(int count, ref double stdDev)
        {
            Reset();
            stepCountError = false;
            List<double> resultList = new List<double>();
            for (int i = 0; i < count; i++)
            {
                Reset();
                double thieResult = Run();
                if (!executionError)
                    resultList.Add(thieResult);
                else
                    stepCountError = true;
            }
            double mean = calcMean(resultList);
            stdDev = calcStdDev(resultList, mean);
            // calculate stddev
            return mean;
        }

        //  ***************************
        private double calcMean(List<double> l)
        {
            double sum = 0.0;
            foreach (double x in l)
                sum += x;
            return (l.Count == 0 ? 0 : sum / l.Count);
        }
        private double calcStdDev(List<double> l, double mean)
        {
            double sum = 0.0;
            foreach (double x in l)
                sum += Math.Pow(x-mean,2.0);
            return (l.Count < 2 ? 0 : Math.Pow(sum / (l.Count)-1, 0.5) );
        }
        //  ***************************
    }
}
