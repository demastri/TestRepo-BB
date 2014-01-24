using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrocerSim
{
    public class Register
    {
        private List<Customer> queue;
        private int currentStartTime;
        private int currentEndTime;
        protected int processTimeMultiplier;
        
        public bool busy { get { return currentEndTime != -1; } }
        public bool hasNewCustomer { get { return currentEndTime == -1 && queue.Count > 0; } }
        public int queueCount { get { return queue.Count; } }
        
        public void AddToQueue(Customer c) { queue.Add(c); }
        public Customer GetQueueCustomer(int i) { return queue[i]; }

        public Register()
        {
            queue = new List<Customer>();
            currentStartTime = currentEndTime = -1;
            processTimeMultiplier = 1;
        }
        public void CompleteCheckout()
        {
            queue.RemoveAt(0);
            currentStartTime = currentEndTime = -1;
        }
        public void UpdateEndTime(int thisTime)
        {
            if (currentEndTime == -1 && queueCount > 0) // open checkout with a customer
            {
                currentStartTime = thisTime;
                currentEndTime = thisTime + GetQueueCustomer(0).ItemCount * processTimeMultiplier;
            }
        }
        public bool Ending(int thisTime)
        {
            return currentEndTime == thisTime;
        }
    }
    class RegisterTrainee : Register
    {
        public RegisterTrainee()
            : base()
        {
            processTimeMultiplier = 2;
        }
    }
}
