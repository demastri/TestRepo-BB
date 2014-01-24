using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrocerSim
{
    abstract public class Customer
    {
        private int arrivalTime;
        private int nbrOfItems;

        public int ItemCount { get { return nbrOfItems; } }
        public bool Arriving(int thisTime)
        {
            return arrivalTime == thisTime;
        }

        public Customer(int start, int items)
        {
            arrivalTime = start;
            nbrOfItems = items;
        }
        public abstract void PutMeInSortedOrderToPickARegister(List<Customer> c);
        public abstract void PickARegister(List<Register> r);

        public static Customer CustomerFactory(string s)
        {
            string[] custParams = s.Trim().Split(new char[] { ' ', ',', '.', ':', '\t' });
            if (custParams.Length == 0 || (custParams.Length == 1 && custParams[0] == ""))
                return null;
            int start;
            int items;
            if (!(custParams.Count() == 3 &&    // got 3 elements
                custParams[0].Length == 1 && (custParams[0][0] == 'A' || custParams[0][0] == 'B') &&    // type is ok
                Int32.TryParse(custParams[1], out start) && Int32.TryParse(custParams[2], out items)))  // start / end times ok
            {
                Console.WriteLine("Error Parsing Customer Data From Input");
                return null;
            }
            return (custParams[0][0] == 'A' ? (Customer)new CustomerA(start, items) : (Customer)new CustomerB(start, items));
        }
    }
    class CustomerA : Customer
    {
        public CustomerA(int start, int items)
            : base(start, items)
        {
        }
        // in order of & items, then A before B, so A at front of thisItem count, B at back
        override public void PutMeInSortedOrderToPickARegister(List<Customer> c)
        {
            Customer FirstGuyAtLeastAsBigAsMyList = c.Find(delegate(Customer item) { return ItemCount <= item.ItemCount; });
            c.Insert((FirstGuyAtLeastAsBigAsMyList == null ? c.Count : c.IndexOf(FirstGuyAtLeastAsBigAsMyList)), this);
        }
        override public void PickARegister(List<Register> r)
        {
            int ShortestQueueLength = r.Min(item => item.queueCount);
            Register FindRegForShortestLine = r.Find(delegate(Register item) { return item.queueCount == ShortestQueueLength; });
            FindRegForShortestLine.AddToQueue(this);
        }
    }
    class CustomerB : Customer
    {
        public CustomerB(int start, int items)
            : base(start, items)
        {
        }
        // in order of & itens, then A before B, so A at front of thisItem count, B at back
        override public void PutMeInSortedOrderToPickARegister(List<Customer> c)
        {
            Customer FirstGuyBiggerThanMyList = c.Find(delegate(Customer item) { return ItemCount < item.ItemCount; });
            c.Insert((FirstGuyBiggerThanMyList == null ? c.Count : c.IndexOf(FirstGuyBiggerThanMyList)), this);
        }
        override public void PickARegister(List<Register> r)
        {
            Register noLine = r.Find(delegate(Register item) { return item.queueCount == 0; });
            if (noLine != null)
                noLine.AddToQueue(this);
            else
            {
                int LastGuyLeastItemCount = r.Min(item => item.GetQueueCustomer(item.queueCount - 1).ItemCount); // ok - no empty lines, all have folks in queue
                Register FindRegForLeastItemCount = r.Find(delegate(Register item) { return item.GetQueueCustomer(item.queueCount - 1).ItemCount == LastGuyLeastItemCount; });
                FindRegForLeastItemCount.AddToQueue(this);
            }
        }
    }
}
