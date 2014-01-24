using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrocerSim
{
    abstract class Simulator
    {
        public bool inputParseError { get; set; }
        public int currentTime { get; set; }

        public abstract bool Init(string s);
        public abstract bool Done();
        public abstract void AdvanceState();

        public void Run()
        {
            while (!Done())
                AdvanceState();
        }
    }
    
    class ThisSim : Simulator
    {
        List<Register> registers;
        List<Customer> customers;

        int totalNbrOfRegisters;
        int totalNbrOfTrainees;

        public ThisSim() : base()
        {
        }
        override public bool Init(string inputFile)
        {
            totalNbrOfTrainees = 1; // defined here, could be overridden by input
            inputParseError = false;
            registers = new List<Register>();
            customers = new List<Customer>();
            currentTime = 0;

            System.IO.StreamReader f = GetInputFile(inputFile);
            if( f == null )
                return !(inputParseError = true);

            string s = f.ReadLine();

            if (!InitRegisters(s))
                inputParseError = true;

            while (!inputParseError && (s = f.ReadLine()) != null)
            {
                Customer thisCustomer = Customer.CustomerFactory(s);
                if (!(inputParseError = (thisCustomer == null)))
                    customers.Add(thisCustomer);
            }
            return inputParseError;
        }

        private System.IO.StreamReader GetInputFile(string inputFile) 
        {
            System.IO.StreamReader f = null;
            if (inputFile == null)
                Console.WriteLine("No Input File Specified.  Exiting.");
            else if(!System.IO.File.Exists(inputFile))
                Console.WriteLine("Input File Does Not Exist.  Exiting.");
            else 
                f = new System.IO.StreamReader(inputFile);

            return f;
        }


        override public bool Done()
        {
            return !(RegistersAreBusy() || CustomersAreWaiting());
        }
        override public void AdvanceState()
        {
            currentTime++;
            CompleteCheckouts();
            AssignArrivingCustomersToRegisters();
            SetNewCheckoutEndTimes();
        }

        private bool InitRegisters(string s)
        {
            if (s == null || !Int32.TryParse(s, out totalNbrOfRegisters) || totalNbrOfRegisters < 1)
            {
                Console.WriteLine("Could Not Determine Number Of Registers From Input");
                return false;
            }

            for (int i = 0; i < totalNbrOfRegisters; i++)
                registers.Add(i >= totalNbrOfRegisters - totalNbrOfTrainees ? new RegisterTrainee() : new Register());

            return true;
        }

        private void CompleteCheckouts()
        {
            registers.FindAll(delegate(Register r) { return r.Ending(currentTime); }).ForEach(item => item.CompleteCheckout()); ;
        }

        private void AssignArrivingCustomersToRegisters()
        {
            List<Customer> incoming = new List<Customer>();
            customers.FindAll(delegate(Customer c) { return c.Arriving( currentTime ); }).ForEach(item => item.PutMeInSortedOrderToPickARegister(incoming)); ;
            incoming.ForEach(item => item.PickARegister(registers));
            incoming.ForEach(item => customers.Remove(item));
        }
        
        private void SetNewCheckoutEndTimes()
        {
            registers.FindAll(delegate(Register r) { return r.hasNewCustomer; }).ForEach(item => item.UpdateEndTime(currentTime));
        }
        
        private bool RegistersAreBusy()
        {
            return registers.Find(delegate(Register r) { return r.busy; }) != null;
        }
        
        private bool CustomersAreWaiting()
        {
            return customers.Count > 0;
        }
    }
}
