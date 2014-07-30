﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulator;

namespace LMADSim
{
    
    class ThisSim : Simulator.Simulator
    {
        int pickedDoor;
        int winner;
        int shownDoor;
        double swPct;

        public ThisSim() : base()
        {
        }
        override public bool Init(string input)
        {
            inputParseError = false;
            swPct = Convert.ToDouble(input);
            Reset();

            return inputParseError;
        }
        override public void Reset()
        {
            currentTime = 0;
            PickAWinner();
        }
        override public bool Done()
        {
            return currentTime > 2;
        }
        override public void AdvanceState()
        {
            switch( currentTime++ ) {
                case 0:
                    PickADoor();
                    break;
                case 1:
                    ShowADoor();
                    break;
                case 2:
                    DecideToSwitch();
                    result = (pickedDoor == winner ? 1 : 0);
                    break;
            }
        }

        private void PickAWinner()
        {
            pickedDoor = oracle.Next(0, 3);
        }
        private void PickADoor()
        {
            winner = oracle.Next(0, 3);
        }
        private void ShowADoor()
        {        
            // don't actually have to do anything here, but we'll set the variable for completeness
            
            // if they are not on a winner, only one door can be shown
            if (pickedDoor != winner)
                if( (pickedDoor+1)%3 == winner)
                    shownDoor = (pickedDoor + 2) % 3;   // so we'd switch to (pd+1)%3...(winner)
                else
                    shownDoor = (pickedDoor + 1) % 3;   // so we'd switch to (pd+2)%3...(winner)

            // if they are, it doesn't matter which door is showm, use picked+2 % 3
            shownDoor = (pickedDoor + 2) % 3;   // so we'll switch to (pd+1)%3...
        }
        private void DecideToSwitch()
        {
            bool sw = (oracle.NextDouble() <= swPct);

            if( sw ) {
                if( pickedDoor != winner )
                    pickedDoor = winner;
                else
                    pickedDoor = (pickedDoor + 1) % 3;
            }
        }
    }
}
